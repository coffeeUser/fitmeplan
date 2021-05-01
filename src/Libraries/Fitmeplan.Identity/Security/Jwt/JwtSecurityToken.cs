using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;

namespace Fitmeplan.Identity.Security.Jwt
{
    public class JwtSecurityToken
    {
        private readonly string _secret;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public JwtSecurityToken(string secret)
        {
            _secret = secret;
        }

        /// <summary>
        /// Stores the current user in token.
        /// </summary>
        /// <returns></returns>
        public string TokenFromPrincipal(ClaimsPrincipal principal)
        {
            var payload = new Dictionary<string, object> {};

            if (principal != null)
            {
                var claims = principal.Claims.GroupBy(x => x.Type);

                foreach (var claim in claims)
                {
                    payload.Add(claim.Key, string.Join(";", claim.Select(x => x.Value)));
                }

                var token = TokenFromClaims(payload);
                return token;
            }

            return null;
        }

        /// <summary>
        /// Stores the current user in token.
        /// </summary>
        /// <returns></returns>
        public string TokenFromClaims(Dictionary<string, object> claims)
        {
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(claims, _secret);
            return token;
        }
        /// <summary>
        /// Gets the claims set from token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Dictionary<string, object> ClaimsFromToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                var json = decoder.Decode(token, _secret, verify: true);

                var claims = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return claims;
            }

            return null;
        }
        /// <summary>
        /// Gets the principal from token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public ClaimsPrincipal PrincipalFromToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var claims = ClaimsFromToken(token);

                ClaimsIdentity identity;
                if (claims.Any())
                {
                    identity = new ClaimsIdentity("JwtToken", JwtClaimTypes.Name, JwtClaimTypes.Role);  
                }
                else
                {
                    identity = new ClaimsIdentity();
                }

                foreach (var claim in claims)
                {
                    var values = claim.Value.ToString();
                    foreach (var v in values.Split(';'))
                    {
                        identity.AddClaim(new Claim(claim.Key, v));    
                    }
                }

                return new ClaimsPrincipal(identity);
            }

            return null;
        }
    }
}
