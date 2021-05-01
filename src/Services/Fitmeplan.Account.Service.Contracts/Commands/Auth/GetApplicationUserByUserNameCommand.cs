using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Account.Service.Contracts.Commands.Auth
{
    public class GetApplicationUserByUserNameCommand : QueryRequest
    {
        public string UserName { get; set; }
    }
}
