using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Autofac;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RawRabbit;
using RawRabbit.Enrichers.HttpContext;
using RawRabbit.Enrichers.MessageContext;
using Swashbuckle.AspNetCore.Swagger;
using Fitmeplan.Api.Core;
using Fitmeplan.Api.Filters;
using Fitmeplan.Autofac;
using Fitmeplan.Identity.Security.Jwt;
using Fitmeplan.ServiceBus.Azure;
using Fitmeplan.Storage.Local;

namespace Fitmeplan.Api
{
    public class Startup
    {
        protected internal const string EnvironmentName_Test = "test";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
            IdentityServer = Configuration["IdentityUrl"];
            SpaClient = configuration["SpaClient"];
        }

        public string IdentityServer { get; set; }
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }
        public string SpaClient { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();

            services.AddHttpContextAccessor();
            services.AddCors();
            services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                    options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                });

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Iam API", Version = "v1" });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"{IdentityServer}/connect/authorize"),
                                Scopes = new Dictionary<string, string> {
                                    { "api", "API - full access" }
                                }
                            }
                        }
                    });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddTransient<ISwaggerProvider, CommandSwaggerGenerator>();

            ConfigureAuth(services);

            services.AddSingleton<IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
            services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            if (!Env.IsEnvironment(EnvironmentName_Test))
            {
                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = IdentityServer;
                        options.RequireHttpsMetadata = false;
                        options.Audience = "api1";

                        options.TokenValidationParameters = new TokenValidationParameters() {
                            NameClaimType = JwtClaimTypes.Name,
                            RoleClaimType = JwtClaimTypes.Role
                        };
                    });
            }

            services.AddAntiforgery(options => options.Cookie.Name = "x-csrf-token-cookiename");
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public virtual void ConfigureContainer(ContainerBuilder builder)
        {
            if ("RabbitMQTransport".Equals(Configuration["ServiceBus:Transport"], StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterModule(new ServiceBus.RawRabbit.ServiceBusModule(Configuration, p => 
                    p.UseHttpContext()
                        .UseMessageContext(context => context.GetHttpContext()?.GetBusContext())
                        .UseContextForwarding()));
            }
            else
            {
                builder.RegisterAzureServiceBus(Configuration);
            }

            builder.RegisterConfiguredModulesFromAssemblyContaining<AutofacModule>(Configuration);
            builder.RegisterMessageHandlers();
            builder.RegisterDictionaryHandlers();
            if ("Local".Equals(Configuration["StorageSettings:Type"], StringComparison.OrdinalIgnoreCase))
            {
                var storageConfiguration = new LocalStorageConfiguration();
                Configuration.GetSection("LocalStorageSettings").Bind(storageConfiguration);
                builder.Register(context => storageConfiguration).As<LocalStorageConfiguration>().SingleInstance();
                builder.RegisterType<Identity.Security.Jwt.JwtSecurityToken>().WithParameter("secret", Configuration["Auth:JwtSecret"]).AsSelf().SingleInstance();
                builder.RegisterType<TokenProvider>().AsSelf().SingleInstance();
            }
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.Use((context, next) =>
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
                return next();
            });

            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardingOptions.KnownNetworks.Clear(); //its loopback by default
            forwardingOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardingOptions);

            app.UseResponseCompression();

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            //CORS
            app.UseCors(builder => builder
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowAnyHeader());

                app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options  =>
            {
                options .SwaggerEndpoint("/swagger/v1/swagger.json", "Iam API V1");

                options.OAuthClientId("swagger-client");
                options.OAuthAppName("API - Swagger");
            });
        }
    }
}
