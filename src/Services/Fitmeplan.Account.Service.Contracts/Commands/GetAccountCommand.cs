using Fitmeplan.Contracts;
using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Account.Service.Contracts.Commands
{
    public class GetAccountCommand : QueryRequest
    {
        public IdentityDto Data { get; set; }
    }
}
