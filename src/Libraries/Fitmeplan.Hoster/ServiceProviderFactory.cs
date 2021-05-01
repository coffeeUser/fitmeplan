using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fitmeplan.Hoster
{
    /// <summary>
    ///     A factory for creating a <see cref="T:Autofac.ContainerBuilder" /> and an <see cref="T:System.IServiceProvider" />.
    /// </summary>
    public class ServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        private readonly Action<ContainerBuilder> _configurationAction;
        private readonly  Action<IContainer> _containerConfigurationAction;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Autofac.Extensions.DependencyInjection.AutofacServiceProviderFactory" /> class.
        /// </summary>
        /// <param name="configurationAction">Action on a <see cref="T:Autofac.ContainerBuilder" /> that adds component
        /// registrations to the conatiner.</param>
        /// <param name="containerConfigurationAction">The container configuration action.</param>
        public ServiceProviderFactory(Action<ContainerBuilder> configurationAction = null, Action<IContainer> containerConfigurationAction = null)
        {
            _configurationAction = configurationAction ?? (builder => { });
            _containerConfigurationAction = containerConfigurationAction ?? (container => { });
        }

        /// <summary>
        ///     Creates a container builder from an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        /// <returns>A container builder that can be used to create an <see cref="T:System.IServiceProvider" />.</returns>
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);
            _configurationAction(builder);
            return builder;
        }

        /// <summary>
        ///     Creates an <see cref="T:System.IServiceProvider" /> from the container builder.
        /// </summary>
        /// <param name="containerBuilder">The container builder.</param>
        /// <returns>An <see cref="T:System.IServiceProvider" />.</returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }

            var container = containerBuilder.Build();
            _containerConfigurationAction(container);
            return new AutofacServiceProvider(container);
        }
    }
}