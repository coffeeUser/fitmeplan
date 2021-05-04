using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RawRabbit;
using RawRabbit.Enrichers.HttpContext;
using RawRabbit.Enrichers.MessageContext;
using Fitmeplan.IdentityServer.Configuration;
using Fitmeplan.ServiceBus.Azure;
using Fitmeplan.ServiceBus.Core;
using Fitmeplan.ServiceBus.RawRabbit;
using Fitmeplan.Identity.Security.Jwt;
using Fitmeplan.IdentityServer.Quickstart.Account.Endpoints;
using Fitmeplan.IdentityServer.Quickstart.Account.ResponseHandling;
using Fitmeplan.IdentityServer.Quickstart.Account.Validation;
using Fitmeplan.Storage.Redis;

namespace Fitmeplan.IdentityServer
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public string SpaUrl { get; set; }
        public string ApiUrl { get; set; }

        /// <summary>
        /// Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
        /// </summary>
        public int IdentityTokenLifetime { get; set; } = 300;

        /// <summary>
        /// Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
        /// </summary>
        public int AccessTokenLifetime { get; set; } = 14400;

        /// <summary>
        /// Lifetime of mobile access token in seconds (defaults to 86400 seconds / 24 hours)
        /// </summary>
        public int MobileAccessTokenLifetime { get; set; } = 86400;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration, ILogger<Startup> logger)
        {
            _logger = logger;
            Environment = environment;
            Configuration = configuration;
            IdentityTokenLifetime = Configuration.GetValue<int?>("Auth:IdentityTokenLifetime") ?? IdentityTokenLifetime;
            AccessTokenLifetime = Configuration.GetValue<int?>("Auth:AccessTokenLifetime") ?? AccessTokenLifetime;
            MobileAccessTokenLifetime = Configuration.GetValue<int?>("Auth:MobileAccessTokenLifetime") ?? MobileAccessTokenLifetime;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //callbacks urls from config:
            var clientUrls = new Dictionary<string, string>();
            SpaUrl = Configuration.GetValue<string>("SpaClient");
            ApiUrl = Configuration.GetValue<string>("ApiUrl") ?? Configuration.GetValue<string>("SwaggerClient");

            clientUrls.Add("Swagger", ApiUrl);
            clientUrls.Add("SpaBff", SpaUrl);
            //clientUrls.Add("Spa", Configuration.GetValue<string>("SpaClient"));

            services.AddCors();

            //services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);
            services.AddControllersWithViews()
                .AddNewtonsoftJson();

            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
                options.AuthenticationDisplayName = "Windows";
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            });
            builder.AddProfileService<UserProfileService>()
                .AddResourceOwnerValidator<UserResourceOwnerPasswordValidator>()
                // in-memory, json config
                .AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"))
                .AddInMemoryApiResources(Configuration.GetSection("ApiResources"))
                //.AddInMemoryClients(Configuration.GetSection("clients"))
                .AddInMemoryClients(Config.GetClients(clientUrls, IdentityTokenLifetime, AccessTokenLifetime, MobileAccessTokenLifetime))
                .AddDeveloperSigningCredential()
                .AddEndpoint<ActAsEndpoint>("actas_endpoint", new PathString("/connect/actas"));

            //chrome >80 cookie issue
            //https://www.thinktecture.com/en/identity/samesite/prepare-your-identityserver/
            services.ConfigureNonBreakingSameSiteCookies();

            services.AddAuthentication()
                //.AddGoogle(options =>
                //{
                //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                //    // register your IdentityServer with Google at https://console.developers.google.com
                //    // enable the Google+ API
                //    // set the redirect URI to http://localhost:5000/signin-google
                //    options.ClientId = "copy client ID from Google here";
                //    options.ClientSecret = "copy client secret from Google here";
                //})
                ;
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //get client Url and register ClientConfiguration
            var clientConfiguration = new ClientConfiguration { SpaUrl = Configuration.GetValue<string>("SpaClient") };
            builder.Register(context => clientConfiguration).As<ClientConfiguration>().SingleInstance();

            //get token configuration and register TokenConfiguration
            var tokenConfiguration = new TokenConfiguration();
            Configuration.GetSection("Token").Bind(tokenConfiguration);
            builder.Register(context => tokenConfiguration).As<TokenConfiguration>().SingleInstance();

            //get lockout configuration and register LockoutConfiguration
            var lockoutConfiguration = new LockoutConfiguration();
            Configuration.GetSection("Lockout").Bind(lockoutConfiguration);
            builder.Register(context => lockoutConfiguration).As<LockoutConfiguration>().SingleInstance();

            if ("RabbitMQTransport".Equals(Configuration["ServiceBus:Transport"], StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterModule(new ServiceBus.RawRabbit.ServiceBusModule(Configuration, p => 
                    p.UseHttpContext()
                        .UseMessageContext(context => context.GetHttpContext().GetBusContext())
                        .UseContextForwarding()));
            }
            else
            {
                builder.RegisterAzureServiceBus(Configuration);
            }
            
            builder.RegisterType<ServiceBusUserStore>().AsSelf().SingleInstance();
            builder.RegisterType<RedisDataProvider>().As<IRedisDataProvider>().WithParameter("config", Configuration.GetValue<string>("Redis:ConnectionString")).SingleInstance();
            builder.RegisterType<EmailService>().As<IEmailService>().SingleInstance();
            builder.RegisterType<JwtSecurityToken>().WithParameter("secret", tokenConfiguration.Secret).AsSelf().SingleInstance();
            builder.RegisterType<TokenProvider>().AsSelf().SingleInstance();

            builder.RegisterType<ActAsResponseGenerator>().AsSelf().AsImplementedInterfaces();
            builder.RegisterType<ActAsRequestValidator>().AsSelf().AsImplementedInterfaces();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
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

            app.UseIdentityServer();

            // Add this before any other middleware that might write cookies
            //https://www.thinktecture.com/en/identity/samesite/prepare-your-identityserver/
            app.UseCookiePolicy();

            app.UseStaticFiles();
            //app.UseMvcWithDefaultRoute();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors(builder => builder
                .AllowAnyMethod()
                .AllowAnyOrigin()
                //.WithOrigins($"{SpaUrl}", $"{ApiUrl}")
                .AllowAnyHeader());

                app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}