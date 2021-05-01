using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Windows.Input;
using Autofac;
using Microsoft.Extensions.Configuration;
using RawRabbit.Channel.Abstraction;
using RawRabbit.Common;
using RawRabbit.Configuration;
using RawRabbit.Configuration.Exchange;
using RawRabbit.DependencyInjection.Autofac;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Instantiation;
using Fitmeplan.Identity.Security.Jwt;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.ServiceBus.RawRabbit
{
    public class ServiceBusModule : Module
    {
        private IConfiguration Configuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusModule"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ServiceBusModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusModule" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="pluginsOptionsOptions">The plugins options.</param>
        public ServiceBusModule(IConfiguration configuration, Action<IClientBuilder> pluginsOptionsOptions)
        {
            Configuration = configuration;
            PluginsOptions = pluginsOptionsOptions;
        }

        private readonly Action<IClientBuilder> PluginsOptions = p =>
        {
            p.UseContextForwarding()
                .UseMessageContext(context => BusContext.Current ??
                                              (CorrelationContext) CorrelationContext.Create<ICommand>(
                                                  Trace.CorrelationManager.ActivityId,
                                                  Thread.CurrentPrincipal as ClaimsPrincipal,
                                                  Guid.Empty, string.Empty, null, string.Empty))
                ;
        };

        /// <summary>Override to add registrations to the container.</summary>
        /// <remarks>
        ///     Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">
        ///     The builder through which components can be
        ///     registered.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            var options = new ServiceBusConfiguration();
            Configuration.GetSection("ServiceBus").Bind(options);
            builder.Register(context => options).As<ServiceBusConfiguration>().SingleInstance();

            builder.RegisterType<BusContextAccessor>().As<IContextAccessor>().SingleInstance();
            builder.RegisterType<JwtSecurityToken>().WithParameter("secret", Configuration["Auth:JwtSecret"]).AsSelf().SingleInstance();

            NamingConventions namingConventions = new NamingConventions();
            namingConventions.ExchangeNamingConvention = type => type.ToString().Replace("`1", string.Empty);
            namingConventions.QueueNamingConvention = type =>
                $"{options.ApplicationName}:{namingConventions.ExchangeNamingConvention(type).ToLower()}";

            var configuration = ConnectionStringParser.Parse(options.ConnectionString);

            var clientConfiguration = new RawRabbitConfiguration
            {
                Username = configuration.Username,
                Password = configuration.Password,
                VirtualHost = configuration.VirtualHost,
                Hostnames = configuration.Hostnames,
                Port = 5672,
                RequestTimeout = TimeSpan.FromSeconds(20),
                //"PublishConfirmTimeout": "00:00:01",
                RecoveryInterval = TimeSpan.FromSeconds(20),
                //"PersistentDeliveryMode": true,
                AutoCloseConnection = true,
                AutomaticRecovery = true,
                Exchange = new GeneralExchangeConfiguration
                {
                    Durable = false,
                    AutoDelete = false,
                    Type = ExchangeType.Fanout
                },
                Queue = new GeneralQueueConfiguration
                {
                    AutoDelete = false,
                    Durable = false,
                    Exclusive = false
                }
            };

            var rabbitOptions = new RawRabbitOptions
            {
                ClientConfiguration = clientConfiguration,
                DependencyInjection = ioc =>
                {
                    //ioc.AddSingleton<ISerializer, RawRabbitSerializer>();
                    ioc.AddSingleton<INamingConventions>(namingConventions);
                },
                Plugins = PluginsOptions
            };

            builder.RegisterRawRabbit(rabbitOptions);

            builder.RegisterType<RawRabbitServiceBus>().As<IServiceBus>().SingleInstance();

            //inject custom implementations
            builder.RegisterType<ChannelFactoryC>().As<IChannelFactory>().SingleInstance()
                .OnActivating(a =>
                {
                    a.Instance.ConnectAsync()
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                });

            base.Load(builder);
        }
    }
}