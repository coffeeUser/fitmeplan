using System.Collections.Generic;
using Fitmeplan.Contracts;

namespace Fitmeplan.Api.Core.Security
{
    public class SecurityPolicyBuilder
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the requirements.
        /// </summary>
        public IList<SecurityRequirement> Requirements { get; set; } = new List<SecurityRequirement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPolicyBuilder"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SecurityPolicyBuilder(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Requires the specified role and organisation type.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        public SecurityPolicyBuilder Require(Role role)
        {
            Requirements.Add(new SecurityRequirement(role.ToString()));

            return this;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public SecurityPolicy Build()
        {
            return new SecurityPolicy(Name, Requirements);
        }
    }
}