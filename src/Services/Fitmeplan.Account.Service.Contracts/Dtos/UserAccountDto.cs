using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fitmeplan.Common;
using Fitmeplan.Contracts;

namespace Fitmeplan.Account.Service.Contracts.Dtos
{
    public class UserAccountDto : DtoBase
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the forename.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Forename { get; set; }
        
        /// <summary>
        /// Gets or sets the surname.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Surname { get; set; }
        
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [EmailAddress]
        [Required]
        [MaxLength(50)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the organisation identifier.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        [Required]
        public List<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [PhoneNumber]
        [MaxLength(25)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [MinLength(6)]
        [MaxLength(32)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirm password.
        /// </summary>
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the flag "Archived"
        /// </summary>
        public bool Archived { get; set; }
    }
}
