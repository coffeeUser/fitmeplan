using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Extensions.Logging;

namespace Fitmeplan.Hoster
{
    public class HostBuilder<T> : HostBuilder
        where T : StartupBase, new()
    {
        private readonly string _serviceName;
        private readonly StartupBase _startup;

        public HostBuilder(string serviceName)
        {
            _serviceName = serviceName;
            _startup = new T();
        }

        public IHostBuilder Configure(string[] args)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(path);

            var hostBuilder = new HostBuilder()
                    .UseServiceProviderFactory(new ServiceProviderFactory(_ => {}, (container) => _startup.ConfigureEndpoint(container, _serviceName)))
                    .ConfigureServices((context, collection) =>
                    {
                        _startup.ConfigureServices(context, collection);
                    })
                    .ConfigureContainer<ContainerBuilder>((hostBuilderContext, containerBuilder) =>
                    {
                        _startup.ConfigureContainer(hostBuilderContext, containerBuilder);
                    })
                    .ConfigureHostConfiguration(builder =>
                    {
                        Console.Title = _serviceName;
                        builder.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            {"ServiceBus:ApplicationName", _serviceName},
                            {HostDefaults.ApplicationKey, _serviceName}
                        });
                    })
                    .ConfigureAppConfiguration((hostContext, config) =>
                    {
                        //config.AddEnvironmentVariables();
                        config.SetBasePath(Path.Combine(AppContext.BaseDirectory))
                            .AddJsonFile("Configs/appsettings.json", false)
                            .AddJsonFile("Configs/servicesettings.json", true)
                            .AddCommandLine(args)
                            .AddEnvironmentVariables();
                    })
                    .ConfigureLogging((context, builder) =>
                    {
                        //set env var for correct elastic search configuration
                        Environment.SetEnvironmentVariable("ElasticsearchUrl",
                            context.Configuration.GetConnectionString("ElasticsearchUrl"));

                        LogManager.LoadConfiguration("nlog.config");

                        //builder.AddConsole();
                        builder.AddNLog();
                    })
                ;
            return hostBuilder;
        }
    }
}