using System;
using System.Linq;
using Autofac;
using Fitmeplan.Account.Service.Contracts.Commands;
using Fitmeplan.Account.Service.Contracts.Commands.Auth;
using Fitmeplan.Account.Service.Contracts.Dtos;
using Fitmeplan.Api.Core.DictionaryHandlers;
using Fitmeplan.Api.Core.Security;
using Fitmeplan.Contracts;
using Fitmeplan.Contracts.Requests;
using Fitmeplan.Email.Service.Contracts.Commands;
using Fitmeplan.Identity;

namespace Fitmeplan.Api.Core
{
    public static class DependencyExtensions
    {
        /// <summary>
        /// Registers the message handlers.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterMessageHandlers(this ContainerBuilder builder)
        {
            var baseType = typeof(IMessageHandler);
            foreach (Type type in baseType.Assembly.GetExportedTypes().Where(v => !v.IsAbstract && baseType.IsAssignableFrom(v) && !v.IsGenericType))
            {
                builder.RegisterType(type).As<IMessageHandler>().As<IOperationDescription>().SingleInstance();    
            }

            AddAuthorization(builder);
            RegisterAccountCommands(builder);
            RegisterEmailCommands(builder);

            return builder;
        }

        /// <summary>
        /// Registers the message handlers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="configurePolicy">An action delegate to configure policy.</param>
        /// <returns></returns>
        public static ContainerBuilder AddSecurityPolicy<T>(this ContainerBuilder builder, Action<SecurityPolicyBuilder> configurePolicy) where T : RequestBase
        {
            SecurityPolicyBuilder authorizationPolicyBuilder = new SecurityPolicyBuilder(typeof(T).Name);
            configurePolicy(authorizationPolicyBuilder);
            builder.RegisterInstance(authorizationPolicyBuilder.Build());
            return builder;
        }

        /// <summary>
        /// Registers the dictionary handlers.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterDictionaryHandlers(this ContainerBuilder builder)
        {
            var baseType = typeof(ILookupHandler);
            foreach (Type type in baseType.Assembly.GetExportedTypes().Where(v => !v.IsAbstract && baseType.IsAssignableFrom(v) && !v.IsGenericType))
            {
                builder.RegisterType(type).As<ILookupHandler>().SingleInstance();    
            }

            return builder;
        }

        private static void AddAuthorization(ContainerBuilder builder)
        {
            //account 
            builder.AddSecurityPolicy<GetAccountCommand>(b => b
                .Require(Role.Administrator));

            //auxiliary types
            builder.RegisterType<SecurityPolicyEvaluator>().AsSelf().SingleInstance();
            builder.RegisterType<AuthorizationService>().AsSelf().SingleInstance();
        }

        /// <summary>
        /// Registers the account commands.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void RegisterAccountCommands(ContainerBuilder builder)
        {
            builder.RegisterType<MessageHandler<GetApplicationUserByExternalProviderCommand, ApplicationUser>>().As<IMessageHandler>()
                .As<IOperationDescription>().SingleInstance();
            builder.RegisterType<MessageHandler<GetApplicationUserBySubjectIdCommand, ApplicationUser>>().As<IMessageHandler>()
                .As<IOperationDescription>().SingleInstance();
            builder.RegisterType<MessageHandler<GetApplicationUserByUserNameCommand, ApplicationUser>>().As<IMessageHandler>()
                .As<IOperationDescription>().SingleInstance();
            builder.RegisterType<MessageHandler<ValidateCredentialsCommand, bool>>().As<IMessageHandler>()
                .As<IOperationDescription>().SingleInstance();

            builder.RegisterType<MessageHandler<GetAccountCommand, UserAccountDto>>().As<IMessageHandler>()
                .As<IOperationDescription>().SingleInstance();
           
            builder.RegisterType<MessageHandler<ResetUserPasswordCommand, int>>()
                .WithProperty(nameof(IOperationDescription.IsHybridOperation), true)
                .As<IMessageHandler>().As<IOperationDescription>().SingleInstance();
        }

        /// <summary>
        /// Registers the email commands.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void RegisterEmailCommands(ContainerBuilder builder)
        {
            builder.RegisterType<MessageHandler<SendResetPasswordLinkCommand, string>>()
                .As<IMessageHandler>().As<IOperationDescription>().SingleInstance();
        }
    }
}
