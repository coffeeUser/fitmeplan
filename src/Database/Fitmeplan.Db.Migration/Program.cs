using System;
using System.IO;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Extensions.Logging;

namespace Fitmeplan.Db.Migration
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceName = "fitmeplan-db-migration";

            var logger = LogManager.LoadConfiguration("nlog.config").GetLogger(serviceName);
            GlobalDiagnosticsContext.Set("servicename", serviceName);

            Console.WriteLine($"Run args: {string.Join(",",args)}");

            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder();
                builder.SetBasePath(Path.Combine(AppContext.BaseDirectory))
                    .AddJsonFile("appsettings.json", false)
                    .AddCommandLine(args)
                    .AddEnvironmentVariables();
                var configuration = builder.Build();

                var serviceProvider = CreateServices(configuration);

                // Put the database update into a scope to ensure
                // that all resources will be disposed.
                using (var scope = serviceProvider.CreateScope())
                {
                    var task = configuration["Task"];

                    Console.WriteLine($"Starting task: {task}");

                    if (nameof(CreateDatabase).Equals(task, StringComparison.OrdinalIgnoreCase))
                    {
                        CreateDatabase(scope.ServiceProvider);
                    }

                    if (nameof(DropDatabase).Equals(task, StringComparison.OrdinalIgnoreCase))
                    {
                        DropDatabase(scope.ServiceProvider);
                    }

                    if (nameof(UpdateDatabase).Equals(task, StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateDatabase(scope.ServiceProvider);
                    }

                    if (nameof(ExportDatabase).Equals(task, StringComparison.OrdinalIgnoreCase))
                    {
                        ExportDatabase(scope.ServiceProvider);
                    }
                }
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

        private static void DropDatabase(IServiceProvider services)
        {
            var creator = services.GetRequiredService<IDatabaseCreator>();
            creator.Drop();
        }

        private static void ExportDatabase(IServiceProvider services)
        {
            var creator = services.GetRequiredService<IDatabaseExporter>();
            creator.Run();
        }

        private static void CreateDatabase(IServiceProvider services)
        {
            var creator = services.GetRequiredService<IDatabaseCreator>();
            creator.Run();
            var versionLoader = services.GetRequiredService<IVersionLoader>();
            versionLoader.LoadVersionInfo();
            versionLoader.UpdateVersionInfo(VersionHelper.CalculateValue(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 99, 00), "Database Create");

            var migrations = services.GetRequiredService<IMigrationRunner>().MigrationLoader.LoadMigrations();
            foreach (var migration in migrations)
            {
                versionLoader.UpdateVersionInfo(migration.Key, migration.Value.Description ?? migration.Value.Migration.GetType().Name);
            }
        }

        /// <summary>
        /// Configure the dependency injection services
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        private static IServiceProvider CreateServices(IConfigurationRoot configuration)
        {
            var connectionString = configuration["ConnectionStrings:Default"];

            Console.WriteLine($"ConnectionString: {connectionString}");

            var createDatabaseOptions = LoadOptions<CreateDatabaseOptions>(configuration, "Tasks:CreateDatabase");

            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb =>
                {
                    // Add SQLite support to FluentMigrator
                    rb.AddSqlServer2014()
                        // Set the connection string
                        .WithGlobalConnectionString(connectionString)
                        // Define the assembly containing the migrations
                        .ScanIn(typeof(Program).Assembly).For.Migrations();
                })
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb
                    .AddFluentMigratorConsole()
                    .AddNLog()
                )
                //add configuration
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton(createDatabaseOptions)
                .AddSingleton<IDatabaseCreator, ScriptDatabaseCreator>()
                .AddSingleton<IDatabaseExporter, DatabaseExporter>()
                .AddSingleton<IMigrationRunner, MigrationRunner>()
                // Build the service provider
                .BuildServiceProvider(false);
        }

        private static T LoadOptions<T>(IConfigurationRoot configuration, string section) 
            where T : DatabaseTaskOptionsBase, new()
        {
            var options = new T();
            configuration.GetSection(section).Bind(options);
            options.ConnectionString = configuration["ConnectionStrings:Default"];
            return options;
        }

        /// <summary>
        ///     Update the database
        /// </summary>
        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            
            // Execute the migrations
            runner.MigrateUp();
        }
    }
}