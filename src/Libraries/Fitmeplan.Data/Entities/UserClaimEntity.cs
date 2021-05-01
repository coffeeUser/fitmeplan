using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeplan.Data.Entities
{
    /// <summary>
    /// Object representation for table 'UserClaims'.
    /// </summary>
    [Table("UserClaims", Schema = "Identity")]
    public class UserClaimEntity
    {
        /// <summary>
        /// Gets/Sets the Identity field.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets/Sets the field "ClaimType".
        /// </summary>
        [Column("ClaimType")]
        public string ClaimType { get; set; }

        /// <summary>
        /// Gets/Sets the field "ClaimValue".
        /// </summary>
        [Column("ClaimValue")]
        public string ClaimValue { get; set; }

        /// <summary>
        /// Gets/Sets the field "UserId".
        /// </summary>
        public virtual int UserId { get; set; }
    }
}
