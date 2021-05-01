using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Email.Service.Contracts.Commands
{
    public class SendResetPasswordLinkCommand : CommandRequest
    {
        public string Email { get; set; }
        public string Url { get; set; }
        public bool IsMobileClient { get; set; }
    }
}