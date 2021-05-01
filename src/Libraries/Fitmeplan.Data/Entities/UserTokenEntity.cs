using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeplan.Data.Entities
{
    /// <summary>
    /// Object representation for table 'UserTokens'.
    /// </summary>
    [Table("UserTokens", Schema = "Identity")]
    public class UserTokenEntity
    {
        /// <summary>
        /// Gets/Sets the Identity field.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets/Sets the field "Value".
        /// </summary>
        [Column("Value")]
        public string Value { get; set; }
		
    }
}
