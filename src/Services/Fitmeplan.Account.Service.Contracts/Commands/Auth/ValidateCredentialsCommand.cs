using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Account.Service.Contracts.Commands.Auth
{
    public class ValidateCredentialsCommand : CommandRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
