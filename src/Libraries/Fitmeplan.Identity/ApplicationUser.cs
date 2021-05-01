using Fitmeplan.Identity.Security;

namespace Fitmeplan.Identity
{
    /// <summary>Represents a user in the identity system</summary>
    /// <typeparam name="TKey">The type used for the primary key for the user.</typeparam>
    public class ApplicationUser : ApplicationUserBase, IUser
    {
        /// <summary>
        /// Initializes a new instance of <see cref="T:Microsoft.AspNetCore.Identity.IdentityUser`1" />.
        /// </summary>
        public ApplicationUser()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:Microsoft.AspNetCore.Identity.IdentityUser`1" />.
        /// </summary>
        /// <param name="username">The user name.</param>
        public ApplicationUser(string username)
          : this()
        {
            Username = username;
        }

        /// <summary>
        /// Gets or sets the name of the provider.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the provider subject identifier.
        /// </summary>
        public string ProviderSubjectId { get; set; }

        /// <summary>
        /// Gets or sets the Forename of the user.
        /// </summary>
        public string Forename { get; set; }

        /// <summary>
        /// Gets or sets the Surname of the user.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the subject identifier.
        /// </summary>
        public string SubjectId
        {
            get { return Id.ToString(); }
            set { }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Username;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
