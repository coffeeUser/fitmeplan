using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Account.Service.Contracts.Commands.Auth
{
    public class GetApplicationUserByExternalProviderCommand : QueryRequest
    {
        public string ProviderName { get; set; }
        public string ProviderSubjectId { get; set; }
    }
}
