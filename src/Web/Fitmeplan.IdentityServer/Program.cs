using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Fitmeplan.IdentityServer
{
    public class Program
    {
        protected internal static string ServiceName = "fitmeplan-identityserver";

        public static void Main(string[] args)
        {
            Console.Title = ServiceName;

            GlobalDiagnosticsContext.Set("servicename", ServiceName);

            var logger = LogManager.LoadConfiguration("nlog.config").GetLogger(ServiceName);

            try
            {
                CreateWebHostBuilder(args).Build().Run();
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5005")
                .ConfigureAppConfiguration((hostingContext, config) => config
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"ServiceBus:ApplicationName", ServiceName}
                    })
                    .AddCommandLine(args)
                    .AddEnvironmentVariables()
                )
                .ConfigureServices(services => services.AddAutofac())
                .UseStartup<Startup>()
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
                });
        }
    }
}