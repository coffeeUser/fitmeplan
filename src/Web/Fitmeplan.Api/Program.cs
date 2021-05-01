using System;
using System.Collections.Generic;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Fitmeplan.Api
{
    public class Program
    {
        protected internal static string ServiceName = "fitmeplan-api";

        public static void Main(string[] args)
        {
            Console.Title = ServiceName;

            GlobalDiagnosticsContext.Set("servicename", ServiceName);

            var logger = LogManager.LoadConfiguration("nlog.config").GetLogger(ServiceName);

            try
            {
                var host = CreateWebHostBuilder(args).Build();
                host.Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, ex.Message);
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5000")
                .ConfigureAppConfiguration((hostingContext, config) => config
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"ServiceBus:ApplicationName", ServiceName}
                    })
                    .AddCommandLine(args)
                    .AddEnvironmentVariables()
                )
                .ConfigureLogging((context, builder) =>
                {
                    // clear all previously registered providers
                    builder.ClearProviders();

                    //set env var for correct elastic search configuration
                    Environment.SetEnvironmentVariable("ElasticsearchUrl",
                        context.Configuration.GetConnectionString("ElasticsearchUrl"));

                    LogManager.LoadConfiguration("nlog.config");

                    builder.AddConsole();
                    builder.AddNLog();
                })
                .ConfigureServices(services => services.AddAutofac())
                .UseStartup<Startup>();
    }
}
