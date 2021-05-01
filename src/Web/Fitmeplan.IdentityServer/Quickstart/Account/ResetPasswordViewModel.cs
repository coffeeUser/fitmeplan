using System.ComponentModel.DataAnnotations;

namespace Fitmeplan.IdentityServer.Quickstart.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public bool IsMobileClient { get; set; }

        public string ReturnUrl { get; set; }
    }
}
