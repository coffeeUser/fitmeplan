using System;
using Autofac;
using Fitmeplan.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Fitmeplan.Identity.Security.Jwt;
using Fitmeplan.ServiceBus.Azure;
using Fitmeplan.ServiceBus.RawRabbit;
using Fitmeplan.Storage;
using Fitmeplan.Storage.Azure;
using Fitmeplan.Storage.Client.Ftp;
using Fitmeplan.Storage.Local;
using Fitmeplan.Identity;

namespace Fitmeplan.Account.Service
{
    public class AccountServiceModule : Module
    {
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            var isLocalStorage = false;
            if ("RabbitMQTransport".Equals(Configuration["ServiceBus:Transport"], StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterRabbitMQServiceBus(Configuration);
            }
            else
            {
                builder.RegisterAzureServiceBus(Configuration);
            }

            if ("Local".Equals(Configuration["StorageSettings:Type"], StringComparison.OrdinalIgnoreCase))
            {
                isLocalStorage = true;
                var storageConfiguration = new LocalStorageConfiguration();
                Configuration.GetSection("LocalStorageSettings").Bind(storageConfiguration);
                builder.Register(context => storageConfiguration).As<LocalStorageConfiguration>().SingleInstance();
                builder.RegisterType<LocalStorage>().As<IStorage>().AsSelf().SingleInstance();
                builder.RegisterType<JwtSecurityToken>().WithParameter("secret", Configuration["Auth:JwtSecret"]).AsSelf().SingleInstance();
                builder.RegisterType<TokenProvider>().AsSelf().SingleInstance();
            }
            else
            {
                var storageConfiguration = new AzureStorageConfiguration();
                Configuration.GetSection("StorageSettings").Bind(storageConfiguration);
                builder.Register(context => storageConfiguration).As<AzureStorageConfiguration>().SingleInstance();
                builder.RegisterType<AzureStorage>().As<IStorage>().AsSelf().SingleInstance();
            }

            builder.RegisterType<AuthRepository>().WithParameter("connectionString", Configuration["ConnectionStrings:Default"]).AsSelf().SingleInstance();
            builder.RegisterType<AuthCommandsService>().As<IHostedService>().AsSelf().SingleInstance();

            builder.RegisterType<AccountRepository>().WithParameter("connectionString", Configuration["ConnectionStrings:Default"]).AsSelf().SingleInstance();
            builder.RegisterType<GetAccountCommandService>().As<IHostedService>().AsSelf().SingleInstance();
            builder.RegisterType<ResetUserPasswordCommandService>().As<IHostedService>().AsSelf().SingleInstance();

            builder.RegisterType<PasswordHasher<ApplicationUser>>().As<IPasswordHasher<ApplicationUser>>();
            builder.RegisterType<PasswordHasher<UserEntity>>().As<IPasswordHasher<UserEntity>>();

            //import
            var ftpClientConfiguration = new FtpClientConfiguration();
            Configuration.GetSection("FtpClientSettings").Bind(ftpClientConfiguration);
            builder.Register(context => ftpClientConfiguration).As<FtpClientConfiguration>().SingleInstance();

            builder.RegisterType<WebClientProvider>().WithParameter("isLocalStorage", isLocalStorage)
                .As<IWebClientProvider>().AsSelf().SingleInstance();

            builder.RegisterType<FtpClient>().AsSelf().SingleInstance();

            base.Load(builder);
        }
    }
}
