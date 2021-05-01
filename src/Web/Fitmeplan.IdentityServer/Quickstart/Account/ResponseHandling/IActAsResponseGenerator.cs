using System.Threading.Tasks;
using Fitmeplan.IdentityServer.Quickstart.Account.Validation.Results;

namespace Fitmeplan.IdentityServer.Quickstart.Account.ResponseHandling
{
    public interface IActAsResponseGenerator
    {
        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        Task<ActAsResponse> ProcessAsync(ActAsRequestValidationResult validationResult);
    }
}
