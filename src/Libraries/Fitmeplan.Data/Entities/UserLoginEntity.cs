using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeplan.Data.Entities
{
    /// <summary>
    /// Object representation for table 'UserLogins'.
    /// </summary>
    [Table("UserLogins", Schema = "Identity")]
    public class UserLoginEntity
    {
        /// <summary>
        /// Gets/Sets the field "LoginProvider".
        /// </summary>
        public string LoginProvider { get; set; }

        /// <summary>
        /// Gets/Sets the field "ProviderKey".
        /// </summary>
        public string ProviderKey { get; set; }

        /// <summary>
        /// Gets/Sets the field "ProviderDisplayName".
        /// </summary>
        [Column("ProviderDisplayName")]
        public string ProviderDisplayName { get; set; }

        /// <summary>
        /// Gets/Sets the field "UserId".
        /// </summary>
        [Column("UserId")]
        public int UserId { get; set; }

    }
}