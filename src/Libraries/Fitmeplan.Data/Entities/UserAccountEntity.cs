using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeplan.Data.Entities
{
    /// <summary>
    /// Object representation for table 'UserAccount'.
    /// </summary>
    [Table("UserAccount", Schema = "Core")]
    public class UserAccountEntity : EntityBase
    {
        /// <summary>
        /// Gets/Sets the Identity field.
        /// </summary>
        public override int Id { get; set; }

        /// <summary>
        /// Gets/Sets the field "Forename".
        /// </summary>
        [Column("Forename")]
        public string Forename { get; set; }

        /// <summary>
        /// Gets/Sets the field "Surname".
        /// </summary>
        [Column("Surname")]
        public string Surname { get; set; }

        /// <summary>
        /// Gets/Sets the field "Role".
        /// </summary>
        [Column("Role")]
        public int Role { get; set; }

        /// <summary>
        /// Gets/Sets the field "IsActive".
        /// </summary>
        [Column("IsActive")]
        public bool IsActive { get; set; }
    }
}
