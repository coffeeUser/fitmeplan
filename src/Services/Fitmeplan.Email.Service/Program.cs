using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;
using Fitmeplan.Hoster;

namespace Fitmeplan.Email.Service
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceName = "email-service";
            GlobalDiagnosticsContext.Set("servicename", serviceName);

            var logger = LogManager.LoadConfiguration("nlog.config").GetLogger(serviceName);

            try
            {
                var builder = new HostBuilder<Startup>(serviceName);
                await builder.Configure(args).RunConsoleAsync();
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
    }
}
