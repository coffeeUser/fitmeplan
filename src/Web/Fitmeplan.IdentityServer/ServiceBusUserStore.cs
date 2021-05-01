using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Fitmeplan.Account.Service.Contracts.Commands;
using Fitmeplan.Account.Service.Contracts.Commands.Auth;
using Fitmeplan.Identity;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.IdentityServer
{
    public class ServiceBusUserStore
    {
        private readonly IServiceBus _serviceBus;

        public ServiceBusUserStore(IServiceBus serviceBus)
        {
            _serviceBus = serviceBus;
        }
        
        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public async Task<bool> ValidateCredentials(string username, string password)
        {
            var response = await _serviceBus.RequestAsync<ValidateCredentialsCommand, ResponseMessage>(new ValidateCredentialsCommand{UserName = username, Password = password});
            return response.Success && (long)response.Data == (long) PasswordVerificationResult.Success;
        }

        /// <summary>
        /// Finds the user by subject identifier.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns></returns>
        public async Task<ApplicationUser> FindBySubjectId(string subjectId)
        {
            var response = await _serviceBus.RequestAsync<GetApplicationUserBySubjectIdCommand, ResponseMessage>(new GetApplicationUserBySubjectIdCommand{SubjectId = subjectId});
            return (ApplicationUser) (response.Success ? response.Data : null);
        }

        /// <summary>
        /// Finds the user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public async Task<ApplicationUser> FindByUsername(string username)
        {
            var response = await _serviceBus.RequestAsync<GetApplicationUserByUserNameCommand, ResponseMessage>(new GetApplicationUserByUserNameCommand{UserName = username});
            return (ApplicationUser) (response.Success ? response.Data : null);
        }

        /// <summary>
        /// Finds the user by external provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public async Task<ApplicationUser> FindByExternalProvider(string provider, string userId)
        {
            var response = await _serviceBus.RequestAsync<GetApplicationUserByExternalProviderCommand, ResponseMessage>(new GetApplicationUserByExternalProviderCommand{ProviderName = provider, ProviderSubjectId = userId});
            return (ApplicationUser) (response.Success ? response.Data : null);
        }

        /// <summary>Automatically provisions a user.</summary>
        /// <param name="provider">The provider.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public ApplicationUser AutoProvisionUser(string provider, string providerUserId, List<Claim> claims)
        {
            List<Claim> source1 = new List<Claim>();
            foreach (Claim claim in claims)
            {
                if (claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    source1.Add(new Claim("name", claim.Value));
                else if (((IDictionary<string, string>)JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap)
                    .ContainsKey(claim.Type))
                    source1.Add(new Claim(
                        ((IDictionary<string, string>)JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap)[claim.Type],
                        claim.Value));
                else
                    source1.Add(claim);
            }

            List<Claim> source2 = source1;
            Func<Claim, bool> func = (Func<Claim, bool>)(x => x.Type == "name");
            if (!source2.Any<Claim>(func))
            {
                Claim claim1 = source1.FirstOrDefault<Claim>((Func<Claim, bool>)(x => x.Type == "given_name"));
                string str1 = claim1 != null ? claim1.Value : (string)null;
                Claim claim2 = source1.FirstOrDefault<Claim>((Func<Claim, bool>)(x => x.Type == "family_name"));
                string str2 = claim2 != null ? claim2.Value : (string)null;
                if (str1 != null && str2 != null)
                    source1.Add(new Claim("name", str1 + " " + str2));
                else if (str1 != null)
                    source1.Add(new Claim("name", str1));
                else if (str2 != null)
                    source1.Add(new Claim("name", str2));
            }

            string uniqueId = CryptoRandom.CreateUniqueId(32);
            Claim claim3 = source1.FirstOrDefault<Claim>((Func<Claim, bool>)(c => c.Type == "name"));
            string str = (claim3 != null ? claim3.Value : (string)null) ?? uniqueId;
            ApplicationUser testUser = new ApplicationUser()
            {
                Username = str,
                ProviderName = provider,
                ProviderSubjectId = providerUserId,
            };
            return testUser;
        }

        /// <summary>
        /// Resets the users password.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The user password.</param>
        /// <returns></returns>
        public async Task<ResponseMessage> ResetUserPassword(int userId, string password)
        {
            return await _serviceBus.RequestAsync<ResetUserPasswordCommand, ResponseMessage>(new ResetUserPasswordCommand { UserId = userId, Password = password });
        }
    }
}