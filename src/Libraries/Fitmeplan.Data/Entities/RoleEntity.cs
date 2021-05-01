using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeplan.Data.Entities
{
    /// <summary>
    /// Object representation for table 'Roles'.
    /// </summary>
    [Table("Roles", Schema = "Identity")]
    public class RoleEntity
    {
        /// <summary>
        /// Gets/Sets the Identity field.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets/Sets the field "Name".
        /// </summary>
        [Column("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets/Sets the field "NormalizedName".
        /// </summary>
        [Column("NormalizedName")]
        public string NormalizedName { get; set; }

        /// <summary>
        /// Gets/Sets the field "ConcurrencyStamp".
        /// </summary>
        [Column("ConcurrencyStamp")]
        public string ConcurrencyStamp { get; set; }
		
    }
}
