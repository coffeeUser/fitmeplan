using System.ComponentModel.DataAnnotations;

namespace Fitmeplan.IdentityServer.Quickstart.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool IsMobileClient { get; set; }

        public string ReturnUrl { get; set; }
    }
}
