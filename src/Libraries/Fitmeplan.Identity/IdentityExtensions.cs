using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using IdentityModel;

namespace Fitmeplan.Identity
{
    public static class IdentityExtensions
    {
        public static int GetUserId(this IPrincipal principal)
        {
            return (principal as ClaimsPrincipal)?.GetUserId<int>() ?? default(int);
        }

        public static T GetUserId<T>(this ClaimsPrincipal principal) where T : IConvertible
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var id = principal.FindFirst(JwtClaimTypes.Subject);
            if (id != null)
            {
                return (T)Convert.ChangeType(id.Value, typeof(T), CultureInfo.InvariantCulture);
            }
            return default(T);
        }

        public static int? GetOrganisationId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var id = principal.FindFirst(IamClaims.CompanyIdentifier);
            if (!string.IsNullOrEmpty(id?.Value))
            {
                return (int)Convert.ChangeType(id.Value, typeof(int), CultureInfo.InvariantCulture);
            }
            return default(int?);
        }

        public static short? GetOrganisationRole(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var id = principal.FindFirst(IamClaims.CompanyType);
            if (!string.IsNullOrEmpty(id?.Value))
            {
                return (short)Convert.ChangeType(id.Value, typeof(short), CultureInfo.InvariantCulture);
            }
            return default(short?);
        }

        public static bool IsOrganisationRole(this ClaimsPrincipal principal, short organisationRole)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var role = principal.GetOrganisationRole();
            if (role.HasValue)
            {
                return (role.Value & organisationRole) == organisationRole;
            }
            return false;
        }

        public static string GetUserName(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            var ci = identity as ClaimsIdentity;
            var claim = ci?.FindFirst(ci.NameClaimType);
            if (claim != null)
            {
                return Convert.ToString(claim.Value, CultureInfo.InvariantCulture);
            }
            return default(string);
        }

        public static string GetOrganisationTimeZone(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var timeZone = principal.FindFirst(JwtClaimTypes.ZoneInfo);
            if (timeZone != null && !string.IsNullOrEmpty(timeZone.Value))
            {
                return Convert.ToString(timeZone.Value, CultureInfo.InvariantCulture);
            }
            return default(string);
        }
    }
}