using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Fitmeplan.ServiceBus.RawRabbit;

namespace Fitmeplan.Hoster
{
    public class StartupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupBase"/> class.
        /// </summary>
        public StartupBase()
        {
        }

        /// <summary>
        /// Configures the container.
        /// </summary>
        /// <param name="hostBuilderContext">The context.</param>
        /// <param name="builder">The builder.</param>
        public virtual void ConfigureContainer(HostBuilderContext hostBuilderContext, ContainerBuilder builder)
        {
            builder.RegisterModule(new ServiceBusModule(hostBuilderContext.Configuration));
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="collection">The collection.</param>
        public virtual void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
        {
            
        }

        /// <summary>
        /// Configures the endpoint.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceName"></param>
        public virtual void ConfigureEndpoint(IContainer container, string serviceName)
        {
            
        }
    }
}