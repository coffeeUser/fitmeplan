using System;
using System.Linq;
using Fitmeplan.Contracts;
using Fitmeplan.Identity;

namespace Fitmeplan.Account.Service
{
    public class AuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Finds the by username.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public ApplicationUser FindByUsername(string userName)
        {
            var normalizedUserName = userName.ToUpper();
            ApplicationUser applicationUser;

            using (var context = new AccountDbContext(_connectionString))
            {
                applicationUser =
                        (from usr in context.UserEntities
                        join acc in context.UserAccountEntities on usr.Id equals acc.Id into res1
                        from ua in res1.DefaultIfEmpty()
                        join lgn in context.UserLoginEntities on ua.Id equals lgn.UserId into res2
                        from ul in res2.DefaultIfEmpty()
                        where usr.NormalizedUserName == normalizedUserName || usr.NormalizedEmail == normalizedUserName
                        select new ApplicationUser
                        {
                            Id = usr.Id,
                            IsActive = ua.IsActive,
                            Forename = ua.Forename,
                            Surname = ua.Surname,
                            Role = ((Role) ua.Role).ToString(),
                            ProviderName = ul.ProviderDisplayName,
                            ProviderSubjectId = ul.ProviderKey,
                            Username = usr.UserName,
                            NormalizedUserName = usr.NormalizedUserName,
                            Email = usr.Email,
                            NormalizedEmail = usr.NormalizedEmail,
                            EmailConfirmed = usr.EmailConfirmed,
                            PasswordHash = usr.PasswordHash,
                            SecurityStamp = usr.SecurityStamp,
                            PhoneNumber = usr.PhoneNumber,
                            PhoneNumberConfirmed = usr.PhoneNumberConfirmed,
                            TwoFactorEnabled = usr.TwoFactorEnabled,
                            LockoutEnd = usr.LockoutEnd,
                            LockoutEnabled = usr.LockoutEnabled,
                            AccessFailedCount = usr.AccessFailedCount
                        }).FirstOrDefault();
            }

            return applicationUser;
        }

        /// <summary>
        /// Finds the by subject identifier.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns></returns>
        public ApplicationUser FindBySubjectId(string subjectId)
        {
            var id = Convert.ToInt32(subjectId);
            ApplicationUser applicationUser;

            using (var context = new AccountDbContext(_connectionString))
            {
                applicationUser =
                        (from usr in context.UserEntities
                        join acc in context.UserAccountEntities on usr.Id equals acc.Id into res1
                        from ua in res1.DefaultIfEmpty()
                        join lgn in context.UserLoginEntities on ua.Id equals lgn.UserId into res2
                        from ul in res2.DefaultIfEmpty()
                        where usr.Id == id
                        select new ApplicationUser
                        {
                            Id = usr.Id,
                            IsActive = ua.IsActive,
                            Forename = ua.Forename,
                            Surname = ua.Surname,
                            Role = ((Role)ua.Role).ToString(),
                            ProviderName = ul.ProviderDisplayName,
                            ProviderSubjectId = ul.ProviderKey,
                            Username = usr.UserName,
                            NormalizedUserName = usr.NormalizedUserName,
                            Email = usr.Email,
                            NormalizedEmail = usr.NormalizedEmail,
                            EmailConfirmed = usr.EmailConfirmed,
                            PasswordHash = usr.PasswordHash,
                            SecurityStamp = usr.SecurityStamp,
                            PhoneNumber = usr.PhoneNumber,
                            PhoneNumberConfirmed = usr.PhoneNumberConfirmed,
                            TwoFactorEnabled = usr.TwoFactorEnabled,
                            LockoutEnd = usr.LockoutEnd,
                            LockoutEnabled = usr.LockoutEnabled,
                            AccessFailedCount = usr.AccessFailedCount
                        }).FirstOrDefault();
            }

            return applicationUser;
        }

        /// <summary>
        /// Finds the by external provider.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerSubjectId">The provider subject identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ApplicationUser FindByExternalProvider(string providerName, string providerSubjectId)
        {
            ApplicationUser applicationUser;

            using (var context = new AccountDbContext(_connectionString))
            {
                applicationUser =
                        (from usr in context.UserEntities
                        join acc in context.UserAccountEntities on usr.Id equals acc.Id into res1
                        from ua in res1.DefaultIfEmpty()
                        join lgn in context.UserLoginEntities on ua.Id equals lgn.UserId into res2
                        from ul in res2.DefaultIfEmpty()
                        where ul.ProviderDisplayName == providerName || ul.ProviderKey == providerSubjectId
                        select new ApplicationUser
                        {
                            Id = usr.Id,
                            IsActive = ua.IsActive,
                            Forename = ua.Forename,
                            Surname = ua.Surname,
                            Role = ((Role)ua.Role).ToString(),
                            ProviderName = ul.ProviderDisplayName,
                            ProviderSubjectId = ul.ProviderKey,
                            Username = usr.UserName,
                            NormalizedUserName = usr.NormalizedUserName,
                            Email = usr.Email,
                            NormalizedEmail = usr.NormalizedEmail,
                            EmailConfirmed = usr.EmailConfirmed,
                            PasswordHash = usr.PasswordHash,
                            SecurityStamp = usr.SecurityStamp,
                            PhoneNumber = usr.PhoneNumber,
                            PhoneNumberConfirmed = usr.PhoneNumberConfirmed,
                            TwoFactorEnabled = usr.TwoFactorEnabled,
                            LockoutEnd = usr.LockoutEnd,
                            LockoutEnabled = usr.LockoutEnabled,
                            AccessFailedCount = usr.AccessFailedCount
                        }).FirstOrDefault();
            }

            return applicationUser;
        }
    }
}