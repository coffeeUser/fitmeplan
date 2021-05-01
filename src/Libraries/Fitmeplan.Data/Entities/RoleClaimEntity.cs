using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeplan.Data.Entities
{
    /// <summary>
    /// Object representation for table 'RoleClaims'.
    /// </summary>
    [Table("RoleClaims", Schema = "Identity")]
    public class RoleClaimEntity
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
        /// Gets/Sets the field "RoleId".
        /// </summary>
        [Column("RoleId")]
        public int RoleId { get; set; }

    }
}
