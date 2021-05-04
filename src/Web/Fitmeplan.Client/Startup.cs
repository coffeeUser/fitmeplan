using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProxyKit;

namespace Fitmeplan.Client
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        public string IdentityUrl { get; }
        public string ApiUrl { get; }

        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
            Configuration = configuration;
            Env = env;

            IdentityUrl = Configuration.GetValue<string>(nameof(IdentityUrl));
            ApiUrl = Configuration.GetValue<string>(nameof(ApiUrl));

            Console.WriteLine(IdentityUrl);
            Console.WriteLine(ApiUrl);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProxy();

            services.AddCors();

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddControllersWithViews().AddNewtonsoftJson();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                if (Env.IsDevelopment())
                {
                    configuration.RootPath = "ClientApp/dist";
                }
                {
                    configuration.RootPath = "wwwroot/dist";
                }
            });

            //chrome >80 cookie issue
            //https://www.thinktecture.com/en/identity/samesite/prepare-your-identityserver/
            services.ConfigureNonBreakingSameSiteCookies();

            ConfigureAuth(services);
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("Cookies", options =>
                {
                    options.Cookie.Name = "bff";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                })
                .AddAutomaticTokenManagement()
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = IdentityUrl;
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "fitmeplan-client-app";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("email");
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("api");
                    options.Scope.Add("offline_access");
                    options.ClaimActions.Add(new JsonKeyClaimAction("role", "role", "role"));
                    options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role
                    };

                    //handling "Correlation failed" and "Unable to unprotect" exceptions according to tasks ALOTE-273 and ALOTE-274.
                    //this exceptions caused by invalid oidc tokens (time expiration or missed decryption keys in the application rebuild case).
                    options.Events.OnRemoteFailure = context =>
                    {
                        if (context.Failure.Message.Contains("Correlation failed") ||
                            context.Failure.Message.Contains("Unable to unprotect"))
                        {
                            _logger.Log(LogLevel.Warning, context.Failure, context.Failure.Message);
                            context.Response.Redirect("/");
                        }
                        else
                        {
                            context.Response.Redirect("/Error");
                        }

                        context.HandleResponse();

                        return Task.CompletedTask;
                    };
                });

            services.AddAntiforgery(options => options.Cookie.Name = "x-csrf-token-cookiename");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost | ForwardedHeaders.All
            };
            forwardingOptions.KnownNetworks.Clear(); //its loopback by default
            forwardingOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardingOptions);

            app.Use(async (context, next) =>
            {
                if (Configuration.GetValue<bool>("EnableLogRequestInfo", true))
                {
                    StringBuilder builder = new StringBuilder();

                    // Request method, scheme, and path
                    builder.AppendLine($"Request Method: {context.Request.Method}");
                    builder.AppendLine($"Request Scheme: {context.Request.Scheme}");
                    builder.AppendLine($"Request Path: {context.Request.Path}");
                    // Connection: RemoteIp
                    builder.AppendLine($"Request RemoteIp: {context.Connection.RemoteIpAddress}");

                    // Headers
                    foreach (var header in context.Request.Headers)
                    {
                        builder.AppendLine($"Header: {header.Key}: {header.Value}");
                    }

                    _logger.LogWarning("Request info: \n" + builder);
                }

                await next();
            });

            app.UseMiddleware<StrictSameSiteExternalAuthenticationMiddleware>();

            // Add this before any other middleware that might write cookies
            //https://www.thinktecture.com/en/identity/samesite/prepare-your-identityserver/
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                var hasExtension = Path.HasExtension(context.Request.Path);
                if (!hasExtension)
                {
                    context.Response.Headers["Cache-Control"] = "no-cache";
                    context.Response.Headers["Pragma"] = "no-cache";
                }

                if (!context.User.Identity.IsAuthenticated
                    && !context.Request.Path.StartsWithSegments("/app")
                    && !context.Request.Path.StartsWithSegments("/api")
                    && !context.Request.Path.StartsWithSegments("/user")
                    && !hasExtension
                    )
                {
                    await context.ChallengeAsync();
                }
                else
                {
                    await next();
                }
            });

            app.Map("/api", api =>
            {
                api.RunProxy(async context =>
                {
                    var forwardContext = context.ForwardTo($"{ApiUrl}/api");

                    var token = await context.GetTokenAsync("access_token");
                    //forwardContext.UpstreamRequest.Headers.Add("Authorization", "Bearer " + token);
                    forwardContext.UpstreamRequest.SetToken(token);

                    return await forwardContext.Send();
                });
            });

            //app.UseHttpsRedirection();
            //DefaultFilesOptions options = new DefaultFilesOptions();
            //options.DefaultFileNames.Clear();
            //options.DefaultFileNames.Add("dist/index.html");
            //app.UseDefaultFiles(options);
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller}/{action=Index}/{id?}");
            //    routes.MapSpaFallbackRoute(
            //        name: "spa-fallback",
            //        defaults: new { controller = "Home", action = "Index" });
            //});
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(builder => builder
                .AllowAnyMethod()
                //.AllowAnyOrigin()
                .WithOrigins($"{IdentityUrl}", $"{ApiUrl}")
                .AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapFallbackToController("Index", "Home");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });

        }
    }
}
