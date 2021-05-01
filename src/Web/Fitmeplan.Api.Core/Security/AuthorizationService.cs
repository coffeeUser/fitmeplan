using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api.Core.Security
{
    public class AuthorizationService
    {
        private readonly IContextAccessor _contextAccessor;
        private readonly SecurityPolicyEvaluator _policyEvaluator;
        private readonly Dictionary<string, SecurityPolicy> _policiesMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"></see> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        /// <param name="policies">The policies.</param>
        /// <param name="policyEvaluator">The policy evaluator.</param>
        public AuthorizationService(IContextAccessor contextAccessor, IEnumerable<SecurityPolicy> policies, SecurityPolicyEvaluator policyEvaluator)
        {
            _contextAccessor = contextAccessor;
            _policyEvaluator = policyEvaluator;

            _policiesMap = policies.ToDictionary(x => x.Name.ToLower());
        }

        /// <summary>
        /// Checks if a user meets a specific set of requirements for the specified resource.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public Task<AuthorizationResult> AuthorizeAsync(string operationName)
        {
            if (_policiesMap.TryGetValue(operationName.ToLower(), out var policy))
            {
                return Task.FromResult(_policyEvaluator.Evaluate(_contextAccessor.CurrentPrincipal, policy));
            }

            return Task.FromResult(AuthorizationResult.Success());
        }
    }
}