using System.Threading.Tasks;
using Fitmeplan.Email.Service.Contracts.Dtos;

namespace Fitmeplan.Email.Service
{
    public interface ITemplateProvider
    {
        Task<string> GetResetPasswordTemplateAsync(ResetPasswordEmailTemplateDto dto);
    }
}
