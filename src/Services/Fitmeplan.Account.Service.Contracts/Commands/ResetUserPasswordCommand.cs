using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Account.Service.Contracts.Commands
{
    public class ResetUserPasswordCommand : CommandRequest
    {
        public int UserId { get; set; }
        public string Password { get; set; }
    }
}
