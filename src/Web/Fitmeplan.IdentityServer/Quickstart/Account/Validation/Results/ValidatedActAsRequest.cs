using IdentityServer4.Validation;
using System.Collections.Generic;

namespace Fitmeplan.IdentityServer.Quickstart.Account.Validation.Results
{
    /// <summary>
    /// Models a validated request to the token endpoint.
    /// </summary>
    public class ValidatedActAsRequest : ValidatedRequest
    {
        /// <summary>
        /// Gets or sets the type of the grant.
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the principal identifier.
        /// </summary>
        /// <value>The principal identifier.</value>
        public string PrincipalId { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>The scopes.</value>
        public List<string> Scopes { get; set; }
    }
}
