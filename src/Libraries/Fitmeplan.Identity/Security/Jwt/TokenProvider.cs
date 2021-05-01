using System.Collections.Generic;

namespace Fitmeplan.Identity.Security.Jwt
{
    public class TokenProvider
    {
        private readonly JwtSecurityToken _securityToken;

        public TokenProvider(JwtSecurityToken securityToken)
        {
            _securityToken = securityToken;
        }

        public string GenerateSecurityToken(Dictionary<string, object> claims)
        {
            return _securityToken.TokenFromClaims(claims);
        }

        public Dictionary<string, object> GetClaimsFromToken(string token)
        {
            return _securityToken.ClaimsFromToken(token);
        }
    }
}
