using System.Collections.Generic;

namespace Fitmeplan.IdentityServer.Quickstart.Account.ResponseHandling
{
    public class ActAsResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the access token lifetime.
        /// </summary>
        /// <value>
        /// The access token lifetime.
        /// </value>
        public int AccessTokenLifetime { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>The claims.</value>
        public Dictionary<string, object> Claims { get; set; }
    }
}
