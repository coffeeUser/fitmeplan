using System.Threading.Tasks;
using Fitmeplan.Email.Service.Contracts.Commands;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.IdentityServer
{
    public class EmailService : IEmailService
    {
        private readonly IServiceBus _serviceBus;

        public EmailService(IServiceBus serviceBus)
        {
            _serviceBus = serviceBus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <param name="url">The reset password link.</param>
        /// <param name="isMobileClient">The mobile client marker.</param>
        /// <returns></returns>
        public async Task<ResponseMessage> SendResetPasswordEmail(string email, string url, bool isMobileClient)
        {
            var response = await _serviceBus.RequestAsync<SendResetPasswordLinkCommand, ResponseMessage>(new SendResetPasswordLinkCommand { Email = email, Url = url, IsMobileClient = isMobileClient});
            return response;
        }
    }
}
