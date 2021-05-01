using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using RawRabbit.Instantiation;

namespace Fitmeplan.ServiceBus.RawRabbit
{
    public static class DependancyExtensions
    {
        /// <summary>
        /// Registers the transaction handlers.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="conf">The conf.</param>
        /// <param name="pluginsConfigureDelegate">The plugins configure delegate.</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterRabbitMQServiceBus(this ContainerBuilder builder, IConfiguration conf, Action<IClientBuilder> pluginsConfigureDelegate = null)
        {
            builder.RegisterModule(new ServiceBusModule(conf));
            return builder;
        }
    }
}
