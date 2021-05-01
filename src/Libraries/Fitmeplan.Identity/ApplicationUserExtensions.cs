using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;

namespace Fitmeplan.Identity
{
    public static class ApplicationUserExtensions
    {
        public static ClaimsIdentity ClaimsIdentity(this ApplicationUser user, string authenticationType)
        {
            ClaimsIdentity identity = new ClaimsIdentity(authenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);

            var claims = user.GetClaims();

            foreach (var claim in claims)
            {
                identity.AddClaim(claim);
            }
            return identity;
        }

        public static ClaimsPrincipal ClaimsPrincipal(this ApplicationUser user, string authenticationType)
        {
            return new ClaimsPrincipal(user.ClaimsIdentity(authenticationType));
        }

        public static List<Claim> GetClaims(this ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(JwtClaimTypes.Id, user.Id.ToString()));
            claims.Add(new Claim(JwtClaimTypes.Subject, user.Id.ToString()));
            claims.Add(new Claim(JwtClaimTypes.Name, user.Username));
            claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtClaimTypes.PreferredUserName, string.Concat(user.Forename, " ", user.Surname) ?? string.Empty));
            claims.Add(new Claim(JwtClaimTypes.Role, user.Role));

            return claims;
        }
    }
}
