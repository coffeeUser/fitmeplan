using Autofac;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Hosting;
using Fitmeplan.ServiceBus.Azure;
using Fitmeplan.ServiceBus.RawRabbit;

namespace Fitmeplan.Email.Service
{
    public class EmailServiceModule : Module
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
            var resetPasswordConfiguration = new ResetPasswordConfiguration();
            Configuration.GetSection("ResetPasswordConfiguration").Bind(resetPasswordConfiguration);
            builder.Register(context => resetPasswordConfiguration).As<ResetPasswordConfiguration>().SingleInstance();

            if ("RabbitMQTransport".Equals(Configuration["ServiceBus:Transport"], StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterRabbitMQServiceBus(Configuration);
            }
            else
            {
                builder.RegisterAzureServiceBus(Configuration);
            }

            builder.RegisterType<SendResetPasswordLinkCommandService>().As<IHostedService>().AsSelf().SingleInstance();

            builder.RegisterType<EmailProvider>().As<IEmailProvider>()
                .WithParameter(new NamedParameter("userName", Configuration["Smtp:UserName"]))
                .WithParameter(new NamedParameter("password", Configuration["Smtp:Password"]))
                .WithParameter(new NamedParameter("hostName", Configuration["Smtp:HostName"]))
                .WithParameter(new NamedParameter("timeOut", Configuration["Smtp:TimeOut"]))
                .WithParameter(new NamedParameter("serviceEmail", Configuration["Smtp:ServiceEmail"]))
                .WithParameter(new NamedParameter("enableSsl", bool.Parse(Configuration["Smtp:EnableSsl"] ?? "false")))
                .WithParameter(new NamedParameter("port", int.Parse(Configuration["Smtp:Port"] ?? "25")));

            builder.RegisterType<TemplateProvider>().As<ITemplateProvider>().AsSelf().SingleInstance();

            base.Load(builder);
        }
    }
}
