using System.Security.Claims;

namespace Fitmeplan.ServiceBus.Core
{
    public class BusContextAccessor : IContextAccessor
    {
        /// <summary>
        /// Gets the current principal.
        /// </summary>
        /// <value>
        /// The current principal.
        /// </value>
        public ClaimsPrincipal CurrentPrincipal
        {
            get { return BusContext.ClaimsPrincipal; }
        }
    }
}