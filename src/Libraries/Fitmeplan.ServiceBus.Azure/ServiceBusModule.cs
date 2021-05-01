using Autofac;
using Microsoft.Extensions.Configuration;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.ServiceBus.Azure
{
    public class ServiceBusModule : Module
    {
        private IConfiguration Configuration { get; set; }

        public ServiceBusModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

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

            builder.RegisterType<AzureServiceBus>().As<IServiceBus>().SingleInstance();
            base.Load(builder);
        }
    }
}
