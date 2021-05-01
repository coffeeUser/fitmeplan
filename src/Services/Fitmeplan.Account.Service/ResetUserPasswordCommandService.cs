using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Fitmeplan.Account.Service.Contracts.Commands;
using Fitmeplan.Common;
using Fitmeplan.Data.Entities;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Account.Service
{
    public class ResetUserPasswordCommandService : IHostedService
    {
        private readonly IPasswordHasher<UserEntity> _passwordHasher;

        private readonly IServiceBus _serviceBus;
        private readonly AccountRepository _accountRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"></see> class.
        /// </summary>
        /// <param name="serviceBus">The service bus.</param>
        /// <param name="accountRepository">The account repository.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        public ResetUserPasswordCommandService(IServiceBus serviceBus, AccountRepository accountRepository, IPasswordHasher<UserEntity> passwordHasher)
        {
            _serviceBus = serviceBus;
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.SubscribeAsync<ResetUserPasswordCommand, ResponseMessage>(Handle);
        }

        private Task<ResponseMessage> Handle(ResetUserPasswordCommand command)
        {
            var response = new ResponseMessage();
            var user = _accountRepository.GetUserEntity(command.UserId);
            if (user == null)
            {
                throw new NotFoundException();
            }
            user.PasswordHash = _passwordHasher.HashPassword(user, command.Password);
            _accountRepository.SaveUser(user);
            response.Data = user.Id;

            return Task.FromResult(response);
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
