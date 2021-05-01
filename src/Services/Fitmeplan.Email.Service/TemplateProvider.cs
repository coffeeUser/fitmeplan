using System;
using System.IO;
using System.Threading.Tasks;
using RazorLight;
using Fitmeplan.Email.Service.Contracts.Dtos;

namespace Fitmeplan.Email.Service
{
    public class TemplateProvider : ITemplateProvider
    {
        public async Task<string> GetResetPasswordTemplateAsync(ResetPasswordEmailTemplateDto dto)
        {
            var templatePath = Path.Combine(Environment.CurrentDirectory, "Templates");
            var engine = new RazorLightEngineBuilder()
                .UseFilesystemProject(templatePath)
                .UseMemoryCachingProvider()
                .Build();
            var body = await engine.CompileRenderAsync("ResetPasswordEmailBody.cshtml", dto);
            return body;
        }
    }
}
