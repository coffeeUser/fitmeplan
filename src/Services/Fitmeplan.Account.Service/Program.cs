using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;
using Fitmeplan.Hoster;

namespace Fitmeplan.Account.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceName = "account-service";
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