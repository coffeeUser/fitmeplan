using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Fitmeplan.Api.Core.Security;
using Fitmeplan.Contracts;
using Fitmeplan.Identity;

namespace Fitmeplan.Api.Core
{
    public class SecurityPolicyEvaluator
    {
        /// <summary>
        /// Determines whether the authorization result successful or not.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        public AuthorizationResult Evaluate(ClaimsPrincipal claimsPrincipal, SecurityPolicy policy)
        {
            if (policy.Requirements.Any(x => Evaluate(claimsPrincipal, x).Succeeded))
            {
                return AuthorizationResult.Success();
            }
            return AuthorizationResult.Failed();
        }

        public AuthorizationResult Evaluate(ClaimsPrincipal claimsPrincipal, SecurityRequirement requirement)
        {
            if (!claimsPrincipal.IsInRole(requirement.Role))
            {
                return AuthorizationResult.Failed();
            }

            return AuthorizationResult.Success();
        }
    }
}