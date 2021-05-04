using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;

namespace Fitmeplan.Client.Controllers
{
    [Route("connect")]
    [ApiController]
    public class ActAsController : ControllerBase
    {
        private readonly TokenEndpointService _tokenService;

        public ActAsController(TokenEndpointService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("actas")]
        public async Task<IActionResult> ActAsAsync(int targetId)
        {
            if (!IsSuperUser())
            {
                return Forbid();
            }

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _tokenService.GetActAsTokensAsync(accessToken, targetId);
            
            if (response.IsSuccess)
            {
                await SingInAsNewPrincipal(response, actAsAction: true);
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        [Route("endactas")]
        public async Task<IActionResult> EndActAsAsync()
        {
            var original = User.FindFirstValue("originalId");
            if (original == null)
            {
                return Forbid();
            }

            var originalId = Convert.ToInt32(original);
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _tokenService.GetActAsTokensAsync(accessToken, originalId);
            
            if (response.IsSuccess)
            {
                await SingInAsNewPrincipal(response, actAsAction: false);
            }
            else
            {
                return BadRequest();
            }
            
            return Ok();
        }

        private async Task SingInAsNewPrincipal(ActAsResponse response, bool actAsAction = false)
        {
            // getting Authentication service and the Authentication result
            var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            AuthenticateResult authenticateResult = await authService.AuthenticateAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme);

            //sign out current user
            //ALOTE-744 App crashes (page does not load) when SU acting as Provider Admin user, click Back to my account and opens any tab (PROD)
            //https://github.com/dotnet/aspnetcore/issues/4639
            await HttpContext.SignOutAsync();

            // updating tokens in current properties
            AuthenticationProperties properties = authenticateResult.Properties;
            properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, response.refresh_token);
            properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, response.access_token);
            var expiresAt = (DateTime.UtcNow + TimeSpan.FromSeconds(response.expires_in)).ToString("o", CultureInfo.InvariantCulture);
            properties.UpdateTokenValue(OpenIdConnectParameterNames.ExpiresIn, expiresAt);

            // processing user claims from the response and sign in as new principal
            var claims = ProcessClaims(response.claims);
            
            if (actAsAction)
            {
                var originalId = User.GetUserId<string>();
                claims.Add(new Claim("originalId", originalId));
            }

            var idp = authenticateResult.Principal.Claims.First(x => x.Type == "idp").Value;
            claims.Add(new Claim("idp", idp));
            var sid = authenticateResult.Principal.Claims.First(x => x.Type == "sid").Value;
            claims.Add(new Claim("sid", sid));

            var identity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation", "name", "role");

            var principal = new ClaimsPrincipal(identity);

            if (actAsAction || principal.GetRoles().Contains(Role.SuperUser))
            {
                await authService.SignInAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticateResult.Properties);
            }
        }

        private bool IsSuperUser()
        {
            if (User.GetRoles().Contains(Role.SuperUser))
            {
                return true;
            }
            
            return false;
        }

        private List<Claim> ProcessClaims(Dictionary<string, object> dict)
        {
            var claims = new List<Claim>();
            foreach (var item in dict)
            {
                if ("role".Equals(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Value is string)
                    {
                        claims.Add(new Claim(item.Key, (string)item.Value));
                    }
                    else
                    {
                        var roles = JsonConvert.DeserializeObject<List<string>>(item.Value.ToString());
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(item.Key, role));
                        }
                    }
                }
                else
                {
                    claims.Add(new Claim(item.Key, (string)item.Value));
                }
            }

            return claims;
        }
    }
}