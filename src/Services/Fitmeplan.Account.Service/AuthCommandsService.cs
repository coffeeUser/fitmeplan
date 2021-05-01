using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Fitmeplan.Account.Service.Contracts.Commands.Auth;
using Fitmeplan.Data.Entities;
using Fitmeplan.Identity;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Account.Service
{
    public class AuthCommandsService : IHostedService
    {
        private readonly IServiceBus _serviceBus;
        private readonly AuthRepository _repository;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        
        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public AuthCommandsService(IServiceBus serviceBus, AuthRepository repository, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _serviceBus = serviceBus;
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.SubscribeAsync<ValidateCredentialsCommand, ResponseMessage>(ValidateCredentials);
            await _serviceBus.SubscribeAsync<GetApplicationUserByExternalProviderCommand, ResponseMessage>(FindByExternalProvider);
            await _serviceBus.SubscribeAsync<GetApplicationUserBySubjectIdCommand, ResponseMessage>(FindBySubjectId);
            await _serviceBus.SubscribeAsync<GetApplicationUserByUserNameCommand, ResponseMessage>(FindByUsername);
        }

        private Task<ResponseMessage> ValidateCredentials(ValidateCredentialsCommand message)
        {
            var user = _repository.FindByUsername(message.UserName);
            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, message.Password);
                return Task.FromResult(new ResponseMessage(result));
            }
            return Task.FromResult(new ResponseMessage(PasswordVerificationResult.Failed));
        }

        private Task<ResponseMessage> FindByUsername(GetApplicationUserByUserNameCommand request)
        {
            var response = new ResponseMessage();
            var user = _repository.FindByUsername(request.UserName);
            response.Data = user;
            return Task.FromResult(response);
        }

        private Task<ResponseMessage> FindBySubjectId(GetApplicationUserBySubjectIdCommand request)
        {
            var response = new ResponseMessage();
            var user = _repository.FindBySubjectId(request.SubjectId);
            response.Data = user;
            return Task.FromResult(response);
        }

        private Task<ResponseMessage> FindByExternalProvider(GetApplicationUserByExternalProviderCommand request)
        {
            var response = new ResponseMessage();
            var user = _repository.FindByExternalProvider(request.ProviderName, request.ProviderSubjectId);
            response.Data = user;
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
