using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fitmeplan.IdentityServer.Quickstart.Account.Validation.Results;

namespace Fitmeplan.IdentityServer.Quickstart.Account.ResponseHandling
{
    public class ActAsResponseGenerator : IActAsResponseGenerator
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IResourceStore _resourceStore;

        private ActAsResponse _response;

        public ActAsResponseGenerator(ITokenService tokenService, IRefreshTokenService refreshTokenService,
            IResourceStore resourceStore)
        {
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _resourceStore = resourceStore;
        }

        public async Task<ActAsResponse> ProcessAsync(ActAsRequestValidationResult request)
        {
            var (accessToken, refreshToken, claims) = await GetActAsUserInfo(request.ValidatedRequest);

            _response = new ActAsResponse
            {
                AccessToken = accessToken,
                AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
                RefreshToken = refreshToken,
                Claims = ToClaimsDictionary(claims)
            };
            return _response;
        }

        /// <summary>
        /// Creates the access/refresh token.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Client does not exist anymore.</exception>
        protected virtual async Task<(string, string, List<Claim>)> GetActAsUserInfo(
            ValidatedActAsRequest request)
        {
            var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(request.Scopes);
            var createRefreshToken = resources.OfflineAccess;

            var clientClaims = request.ClientClaims.ToList();

            var actAsPrincipal = new IdentityServerUser(request.PrincipalId.Substring(4))
            {
                IdentityProvider = "idp",
                AuthenticationMethods = { "actas" },
                AuthenticationTime = DateTime.UtcNow,
                AdditionalClaims = clientClaims
            }.CreatePrincipal();

            var tokenRequest = new TokenCreationRequest
            {
                Subject = actAsPrincipal,
                Resources = resources,
                ValidatedRequest = request,
            };

            var identityClaimsTypes = resources.IdentityResources.SelectMany(x => x.UserClaims).Distinct().ToList();
            var claims = request.ClientClaims.Where(x =>
                identityClaimsTypes.Any(id => id.Equals(x.Type, StringComparison.InvariantCultureIgnoreCase))).ToList();

            var at = await _tokenService.CreateAccessTokenAsync(tokenRequest);
            var accessToken = await _tokenService.CreateSecurityTokenAsync(at);

            if (createRefreshToken)
            {
                var refreshToken =
                    await _refreshTokenService.CreateRefreshTokenAsync(tokenRequest.Subject, at, request.Client);
                return (accessToken, refreshToken, claims);
            }

            return (accessToken, null, claims);
        }

        private Dictionary<string, object> ToClaimsDictionary(IEnumerable<Claim> claims)
        {
            var d = new Dictionary<string, object>();

            if (claims == null)
            {
                return d;
            }

            var distinctClaims = claims.Distinct(new ClaimComparer());

            foreach (var claim in distinctClaims)
            {
                if (!d.ContainsKey(claim.Type))
                {
                    d.Add(claim.Type, GetValue(claim));
                }
                else
                {
                    var value = d[claim.Type];

                    var list = value as List<object>;
                    if (list != null)
                    {
                        list.Add(GetValue(claim));
                    }
                    else
                    {
                        d.Remove(claim.Type);
                        d.Add(claim.Type, new List<object> { value, GetValue(claim) });
                    }
                }
            }

            return d;
        }

        private object GetValue(Claim claim)
        {
            if (claim.ValueType == ClaimValueTypes.Integer ||
                claim.ValueType == ClaimValueTypes.Integer32)
            {
                if (Int32.TryParse(claim.Value, out var value))
                {
                    return value;
                }
            }

            if (claim.ValueType == ClaimValueTypes.Integer64)
            {
                if (Int64.TryParse(claim.Value, out var value))
                {
                    return value;
                }
            }

            if (claim.ValueType == ClaimValueTypes.Boolean)
            {
                if (bool.TryParse(claim.Value, out var value))
                {
                    return value;
                }
            }

            if (claim.ValueType == IdentityServerConstants.ClaimValueTypes.Json)
            {
                try
                {
                    return JsonConvert.DeserializeObject(claim.Value);
                }
                catch { }
            }

            return claim.Value;
        }
    }
}
