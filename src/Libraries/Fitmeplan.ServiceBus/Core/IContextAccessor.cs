using System.Security.Claims;

namespace Fitmeplan.ServiceBus.Core
{
    public interface IContextAccessor
    {
        /// <summary>
        /// Gets the current principal.
        /// </summary>
        /// <value>
        /// The current principal.
        /// </value>
        ClaimsPrincipal CurrentPrincipal { get; }
    }
}