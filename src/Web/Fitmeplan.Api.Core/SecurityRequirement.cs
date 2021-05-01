namespace Fitmeplan.Api.Core
{
    public class SecurityRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityRequirement"/> class.
        /// </summary>
        /// <param name="role">The role.</param>
        public SecurityRequirement(string role)
        {
            Role = role;
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        public string Role { get; }
    }
}