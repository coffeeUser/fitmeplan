using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;

namespace Fitmeplan.Autofac
{
    public static class ModuleRegistrationExtensions
    {
        public static ContainerBuilder RegisterConfiguredModulesFromAssemblyContaining<TType>(
            this ContainerBuilder builder,
            IConfiguration configuration)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var metaBuilder = new ContainerBuilder();

            metaBuilder.RegisterInstance(configuration);
            metaBuilder.RegisterAssemblyTypes(typeof(TType).GetTypeInfo().Assembly)
                .AssignableTo<IModule>()
                .As<IModule>()
                .PropertiesAutowired();

            using (IContainer metaContainer = metaBuilder.Build())
            {
                foreach (IModule module in metaContainer.Resolve<IEnumerable<IModule>>())
                    builder.RegisterModule(module);
            }

            return builder;
        }

        public static ContainerBuilder RegisterConfiguredModulesFromAssembly(
            this ContainerBuilder builder, Assembly assembly,
            IConfiguration configuration)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var metaBuilder = new ContainerBuilder();

            metaBuilder.RegisterInstance(configuration);
            metaBuilder.RegisterAssemblyTypes(assembly)
                .AssignableTo<IModule>()
                .As<IModule>()
                .PropertiesAutowired();

            using (IContainer metaContainer = metaBuilder.Build())
            {
                foreach (IModule module in metaContainer.Resolve<IEnumerable<IModule>>())
                    builder.RegisterModule(module);
            }

            return builder;
        }
    }
}
