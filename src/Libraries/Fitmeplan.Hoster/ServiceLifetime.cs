using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fitmeplan.Hoster
{
    public class ServiceLifetime : IHostApplicationLifetime
    {
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
        private readonly ILogger _logger;

        public ServiceLifetime(ILogger<ServiceLifetime> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Triggered when the application host has fully started and is about to wait
        ///     for a graceful shutdown.
        /// </summary>
        public CancellationToken ApplicationStarted => _startedSource.Token;

        /// <summary>
        ///     Triggered when the application host is performing a graceful shutdown.
        ///     Request may still be in flight. Shutdown will block until this event completes.
        /// </summary>
        public CancellationToken ApplicationStopping => _stoppingSource.Token;

        /// <summary>
        ///     Triggered when the application host is performing a graceful shutdown.
        ///     All requests should be complete at this point. Shutdown will block
        ///     until this event completes.
        /// </summary>
        public CancellationToken ApplicationStopped => _stoppedSource.Token;

        /// <summary>
        ///     Signals the ApplicationStopping event and blocks until it completes.
        /// </summary>
        public void StopApplication()
        {
            lock (_stoppingSource)
            {
                try
                {
                   ExecuteHandlers(_stoppingSource);
                }
                catch (Exception ex)
                {
                    _logger.LogError(7, "An error occurred stopping the application", ex);
                }
            }
        }

        private void ExecuteHandlers(CancellationTokenSource cancel)
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }
            cancel.Cancel(false);
        }
    }
}