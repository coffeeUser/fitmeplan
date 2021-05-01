using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeplan.Data.Entities
{
    /// <summary>
    /// Object representation for table 'Users'.
    /// </summary>
    [Table("Users", Schema = "Identity")]
    public class UserEntity : EntityBase
    {
        /// <summary>
        /// Gets/Sets the Identity field.
        /// </summary>
        public override int Id { get; set; }

        /// <summary>
        /// Gets/Sets the field "UserName".
        /// </summary>
        [Column("UserName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets/Sets the field "NormalizedUserName".
        /// </summary>
        [Column("NormalizedUserName")]
        public string NormalizedUserName { get; set; }

        /// <summary>
        /// Gets/Sets the field "Email".
        /// </summary>
        [Column("Email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets/Sets the field "NormalizedEmail".
        /// </summary>
        [Column("NormalizedEmail")]
        public string NormalizedEmail { get; set; }

        /// <summary>
        /// Gets/Sets the field "EmailConfirmed".
        /// </summary>
        [Column("EmailConfirmed")]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets/Sets the field "PasswordHash".
        /// </summary>
        [Column("PasswordHash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets/Sets the field "SecurityStamp".
        /// </summary>
        [Column("SecurityStamp")]
        public string SecurityStamp { get; set; }

        /// <summary>
        /// Gets/Sets the field "ConcurrencyStamp".
        /// </summary>
        [Column("ConcurrencyStamp")]
        public string ConcurrencyStamp { get; set; }

        /// <summary>
        /// Gets/Sets the field "PhoneNumber".
        /// </summary>
        [Column("PhoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets/Sets the field "PhoneNumberConfirmed".
        /// </summary>
        [Column("PhoneNumberConfirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Gets/Sets the field "TwoFactorEnabled".
        /// </summary>
        [Column("TwoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Gets/Sets the field "LockoutEnd".
        /// </summary>
        [Column("LockoutEnd")]
        public DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Gets/Sets the field "LockoutEnabled".
        /// </summary>
        [Column("LockoutEnabled")]
        public bool LockoutEnabled { get; set; }

        /// <summary>
        /// Gets/Sets the field "AccessFailedCount".
        /// </summary>
        [Column("AccessFailedCount")]
        public int AccessFailedCount { get; set; }
    }
}
