using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Fitmeplan.Account.Service.Contracts.Commands;
using Fitmeplan.Account.Service.Contracts.Dtos;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Account.Service
{
    public class GetAccountCommandService : IHostedService
    {
        private readonly IServiceBus _serviceBus;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public GetAccountCommandService(IServiceBus serviceBus)
        {
            _serviceBus = serviceBus;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.SubscribeAsync<GetAccountCommand, ResponseMessage>(Handle);
        }

        private Task<ResponseMessage> Handle(GetAccountCommand arg)
        {
            return Task.FromResult(new ResponseMessage(new UserAccountDto()));
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
