using Autofac;
using Microsoft.Extensions.Hosting;
using Fitmeplan.Autofac;
using Fitmeplan.Hoster;

namespace Fitmeplan.Account.Service
{
    public class Startup : StartupBase
    {
        /// <summary>
        /// Configures the container.
        /// </summary>
        /// <param name="hostBuilderContext">The context.</param>
        /// <param name="builder">The builder.</param>
        public override void ConfigureContainer(HostBuilderContext hostBuilderContext, ContainerBuilder builder)
        {
            base.ConfigureContainer(hostBuilderContext, builder);
            builder.RegisterConfiguredModulesFromAssemblyContaining<AccountServiceModule>(hostBuilderContext.Configuration);
        }
    }
}