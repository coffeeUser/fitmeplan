using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Fitmeplan.Email.Service.Contracts.Commands;
using Fitmeplan.Email.Service.Contracts.Dtos;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Email.Service
{
    public class SendResetPasswordLinkCommandService : IHostedService
    {
        private readonly IServiceBus _serviceBus;
        private readonly IEmailProvider _emailProvider;
        private readonly ITemplateProvider _templateProvider;
        private readonly ResetPasswordConfiguration _resetPasswordConfiguration;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public SendResetPasswordLinkCommandService(IServiceBus serviceBus,
                                                    IEmailProvider emailProvider,
                                                    ITemplateProvider templateProvider,
                                                    ResetPasswordConfiguration resetPasswordConfiguration)
        {
            _serviceBus = serviceBus;
            _emailProvider = emailProvider;
            _templateProvider = templateProvider;
            _resetPasswordConfiguration = resetPasswordConfiguration;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serviceBus.SubscribeAsync<SendResetPasswordLinkCommand, ResponseMessage>(Handle);
        }

        private async Task<ResponseMessage> Handle(SendResetPasswordLinkCommand data)
        {
            var response = new ResponseMessage { Success = true };
            var body = await _templateProvider.GetResetPasswordTemplateAsync(new ResetPasswordEmailTemplateDto
            {
                Url = data.Url,
                IsMobileClient = data.IsMobileClient
            });
            try
            {
                _emailProvider.SendMail(data.Email, _resetPasswordConfiguration.Subject, body);
            }
            catch
            {
                response.Success = false;
            }

            return response;
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
