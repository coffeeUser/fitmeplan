using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api.Core.Security
{
    public class HttpContextAccessor : IContextAccessor
    {
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public HttpContextAccessor(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Gets the current principal.
        /// </summary>
        /// <value>
        /// The current principal.
        /// </value>
        public ClaimsPrincipal CurrentPrincipal
        {
            get { return _contextAccessor.HttpContext.User; }
        }
    }
}
