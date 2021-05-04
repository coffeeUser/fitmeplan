using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;

namespace Fitmeplan.Client.Controllers
{
    [Route("user")]
    public class UserController : ControllerBase
    {
        [Route("info")]
        public IActionResult GetUser()
        {
            var user = new
            {
                name = User.Identity.Name,
                roles = User.GetRoles(),
                originalId = User.GetOriginalId(),
                fullname = User.GetFullname()
            };

            return new JsonResult(user);
        }

        [Route("logout")]
        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }

    public enum Role
    {
        SuperUser = 1,
        Administrator,
    }

    public static class IdentityExtensions
    {
        public static IEnumerable<Role> GetRoles(this ClaimsPrincipal principal)
        {
            return principal.FindAll(JwtClaimTypes.Role).Select(x => (Role)Enum.Parse(typeof(Role), x.Value, true));
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

        public static string GetOriginalId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var claim = principal.FindFirst(FitmeplanClaims.OriginalId);
            if (claim != null)
            {
                return Convert.ToString(claim.Value, CultureInfo.InvariantCulture);
            }
            return default(string);
        }

        public static string GetFullname(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var claim = principal.FindFirst(JwtClaimTypes.PreferredUserName);
            if (claim != null)
            {
                return Convert.ToString(claim.Value, CultureInfo.InvariantCulture);
            }
            return default(string);
        }
    }

    public static class FitmeplanClaims
    {
        public const string OriginalId = "originalId";
    }
}