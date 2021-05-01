using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using Fitmeplan.Identity;

namespace Fitmeplan.IdentityServer
{
    /// <summary>Profile service for test users</summary>
    /// <seealso cref="T:IdentityServer4.Services.IProfileService" />
    public class UserProfileService : IProfileService
    {
        /// <summary>The logger</summary>
        protected readonly ILogger Logger;

        /// <summary>The users</summary>
        protected readonly ServiceBusUserStore Users;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:IdentityServer4.Test.TestUserProfileService" /> class.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="logger">The logger.</param>
        //public UserProfileService(UserStore users, ILogger<UserProfileService> logger)
        public UserProfileService(ILogger<UserProfileService> logger, ServiceBusUserStore users)
        {
            Users = users;
            Logger = logger;
        }

        /// <summary>
        ///     This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo
        ///     endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.LogProfileRequest(Logger);
            if (context.RequestedClaimTypes.Any())
            {
                var bySubjectId = await Users.FindBySubjectId(context.Subject.GetSubjectId());
                if (bySubjectId != null)
                {
                    context.AddRequestedClaims(bySubjectId.GetClaims());
                }
            }

            context.LogIssuedClaims(Logger);
        }

        /// <summary>
        ///     This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the
        ///     user's account has been deactivated since they logged in).
        ///     (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            Logger.LogDebug("IsActive called from: {caller}", (object) context.Caller);
            var bySubjectId = await Users.FindBySubjectId(context.Subject.GetSubjectId());
            context.IsActive = bySubjectId != null && bySubjectId.IsActive;
        }
    }
}