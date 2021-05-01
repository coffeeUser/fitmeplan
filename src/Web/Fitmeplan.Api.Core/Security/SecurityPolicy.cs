using System.Collections.Generic;

namespace Fitmeplan.Api.Core.Security
{
    public class SecurityPolicy
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the requirements.
        /// </summary>
        public IList<SecurityRequirement> Requirements { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPolicy"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="requirements">The requirements.</param>
        public SecurityPolicy(string name, IList<SecurityRequirement> requirements)
        {
            Name = name;
            Requirements = requirements;
        }
    }
}