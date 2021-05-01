using System.Threading.Tasks;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.IdentityServer
{
    public interface IEmailService
    {
        Task<ResponseMessage> SendResetPasswordEmail(string email, string url, bool isMobileClient);
    }
}
