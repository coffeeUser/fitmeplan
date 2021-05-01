using System;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fitmeplan.IdentityServer.Quickstart.Account.Endpoints
{
    public class EndpointErrorResult : IEndpointResult
    {
        public TokenErrorResponse Response { get; }

        public EndpointErrorResult(TokenErrorResponse error)
        {
            if (IEnumerableExtensions.IsNullOrEmpty(error.Error)) throw new ArgumentNullException(nameof(error.Error), "Error must be set");

            Response = error;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.SetNoCache();

            var dto = new ResultDto
            {
                error = Response.Error,
                error_description = Response.ErrorDescription
            };

            if (Response.Custom.IsNullOrEmpty())
            {
                await context.Response.WriteJsonAsync(dto);
            }
            else
            {
                await context.Response.WriteJsonAsync(JsonConvert.SerializeObject(dto));
            }
        }

        public class ResultDto
        {
            public string error { get; set; }
            public string error_description { get; set; }
        }
    }
}
