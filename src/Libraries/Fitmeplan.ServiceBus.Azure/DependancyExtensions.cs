using Autofac;
using Microsoft.Extensions.Configuration;

namespace Fitmeplan.ServiceBus.Azure
{
    public static class DependancyExtensions
    {
        /// <summary>
        /// Registers the transaction handlers.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="conf">The conf.</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterAzureServiceBus(this ContainerBuilder builder, IConfiguration conf)
        {
            builder.RegisterModule(new ServiceBusModule(conf));
            return builder;
        }
    }
}
