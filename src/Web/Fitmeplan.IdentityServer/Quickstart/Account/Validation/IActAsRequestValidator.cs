using IdentityServer4.Validation;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fitmeplan.IdentityServer.Quickstart.Account.Validation.Results;

namespace Fitmeplan.IdentityServer.Quickstart.Account.Validation
{
    public interface IActAsRequestValidator
    {
        Task<ActAsRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult);
    }
}
