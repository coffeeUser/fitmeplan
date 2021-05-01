using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Fitmeplan.IdentityServer.Quickstart.Account.ResponseHandling;

namespace Fitmeplan.IdentityServer.Quickstart.Account.Results
{
    public class ActAsResult : IEndpointResult
    {
        private readonly ActAsResponse _response;

        public ActAsResult(ActAsResponse response)
        {
            _response = response;
        }
        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();

            var dto = new ResultDto
            {
                access_token = _response.AccessToken,
                refresh_token = _response.RefreshToken,
                expires_in = _response.AccessTokenLifetime,
                token_type = OidcConstants.TokenResponse.BearerTokenType,
                claims = _response.Claims
            };

            await context.Response.WriteJsonAsync(JsonConvert.SerializeObject(dto));
        }

        internal class ResultDto
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public Dictionary<string, object> claims { get; set; }
        }
    }
}
