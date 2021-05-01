using Autofac;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Fitmeplan.Api.Core;
using Fitmeplan.Api.Core.Security;
using Fitmeplan.Identity.Security;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api
{
    public class AutofacModule : Module
    {
        public IConfiguration Configuration { get; set; }

        /// <summary>Override to add registrations to the container.</summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpContextAccessor>().As<IContextAccessor>().SingleInstance();
            builder.RegisterType<UserValidator<IUser>>().As<IUserValidator<IUser>>().SingleInstance();
            builder.RegisterType<MessageHandlerProvider>().AsSelf().SingleInstance();
            builder.RegisterType<MobileMessageHandlerProvider>().AsSelf().SingleInstance();
            builder.RegisterType<DictionaryHandlerProvider>().AsSelf().SingleInstance();
            base.Load(builder);
        }
    }
}