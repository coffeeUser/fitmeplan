using System;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Fitmeplan.Identity;

namespace Fitmeplan.IdentityServer
{
    /// <summary>Resource owner password validator for test users</summary>
    /// <seealso cref="T:IdentityServer4.Validation.IResourceOwnerPasswordValidator" />
    public class UserResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly ISystemClock _clock;
        private readonly ServiceBusUserStore _users;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserResourceOwnerPasswordValidator" /> class.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <param name="clock">The clock.</param>
        public UserResourceOwnerPasswordValidator(ServiceBusUserStore users, ISystemClock clock)
        {
            _users = users;
            _clock = clock;
        }

        /// <summary>Validates the resource owner password credential</summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (await _users.ValidateCredentials(context.UserName, context.Password))
            {
                var byUsername = await _users.FindByUsername(context.UserName);
                var validationContext = context;
                var subjectId = byUsername.Id.ToString();
                if (subjectId == null)
                {
                    throw new ArgumentException("Subject ID not set", "SubjectId");
                }

                var validationResult = new GrantValidationResult(subjectId, "pwd", _clock.UtcNow.UtcDateTime,
                    byUsername.GetClaims(), "local", null);
                validationContext.Result = validationResult;
            }
        }
    }
}
