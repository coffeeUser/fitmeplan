using System;
using System.Collections.Generic;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore
{
    public class TokenEndpointService
    {
        private readonly AutomaticTokenManagementOptions _managementOptions;
        private readonly IOptionsSnapshot<OpenIdConnectOptions> _oidcOptions;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenEndpointService> _logger;

        public TokenEndpointService(
            IOptions<AutomaticTokenManagementOptions> managementOptions,
            IOptionsSnapshot<OpenIdConnectOptions> oidcOptions,
            IAuthenticationSchemeProvider schemeProvider,
            IHttpClientFactory httpClientFactory,
            ILogger<TokenEndpointService> logger)
        {
            _managementOptions = managementOptions.Value;
            _oidcOptions = oidcOptions;
            _schemeProvider = schemeProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync();
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default(CancellationToken));

            var tokenClient = _httpClientFactory.CreateClient("tokenClient");

            return await tokenClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = configuration.TokenEndpoint,

                ClientId = oidcOptions.ClientId,
                ClientSecret = oidcOptions.ClientSecret,
                RefreshToken = refreshToken
            });
        }

        public async Task<TokenRevocationResponse> RevokeTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync();
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default(CancellationToken));

            var tokenClient = _httpClientFactory.CreateClient("tokenClient");

            return await tokenClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = configuration.AdditionalData[OidcConstants.Discovery.RevocationEndpoint].ToString(),
                ClientId = oidcOptions.ClientId,
                ClientSecret = oidcOptions.ClientSecret,
                Token = refreshToken,
                TokenTypeHint = OidcConstants.TokenTypes.RefreshToken
            });
        }

        public async Task<ActAsResponse> GetActAsTokensAsync(string accessToken, int principalId)
        {
            var result = new ActAsResponse { IsSuccess = false };

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_oidcOptions.Get("oidc").Authority);
            var dict = new Dictionary<string, string>
            {
                { "access_token", accessToken },
                { "principal_id", $"sub:{principalId}" },
                { "client_id", "fitmeplan-client-app" },
                { "client_secret", "secret" },
                { "scope", "openid profile email api offline_access" }
            };
            var httpContent = new FormUrlEncodedContent(dict);

            var response = await client.PostAsync("/connect/actas", httpContent);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                result = response.Content.ReadAsAsync<ActAsResponse>().Result;
                result.IsSuccess = true;
            }

            return result;
        }

        private async Task<OpenIdConnectOptions> GetOidcOptionsAsync()
        {
            if (string.IsNullOrEmpty(_managementOptions.Scheme))
            {
                var scheme = await _schemeProvider.GetDefaultChallengeSchemeAsync();
                return _oidcOptions.Get(scheme.Name);
            }
            else
            {
                return _oidcOptions.Get(_managementOptions.Scheme);
            }
        }
    }

    public class ActAsResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public Dictionary<string, object> claims { get; set; }
        public bool IsSuccess { get; set; }
    }
}