using Fitmeplan.Contracts.Requests;

namespace Fitmeplan.Account.Service.Contracts.Commands.Auth
{
    public class GetApplicationUserBySubjectIdCommand : QueryRequest
    {
        public string SubjectId { get; set; }
    }
}
