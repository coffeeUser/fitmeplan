namespace Fitmeplan.Email.Service.Contracts.Dtos
{
    public class ResetPasswordEmailTemplateDto
    {
        public string Url { get; set; }
        public bool IsMobileClient { get; set; }
    }
}
