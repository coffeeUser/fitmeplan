// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Fitmeplan.Contracts;
using Fitmeplan.Identity;
using Fitmeplan.Identity.Security.Jwt;
using Fitmeplan.IdentityServer;
using Fitmeplan.IdentityServer.Configuration;
using Fitmeplan.IdentityServer.Quickstart.Account;
using Fitmeplan.Storage.Redis;
using Resource = Fitmeplan.IdentityServer.Resource;

namespace IdentityServer4.Quickstart.UI
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ServiceBusUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IEmailService _emailService;
        private readonly ClientConfiguration _clientConfiguration;
        private readonly TokenConfiguration _tokenConfiguration;
        private readonly TokenProvider _tokenProvider;
        private readonly LockoutConfiguration _lockoutConfiguration;
        private readonly LockoutService _lockoutService;
        private readonly IConfiguration _configuration;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IEmailService emailService,
            ClientConfiguration clientConfiguration,
            TokenProvider tokenProvider,
            TokenConfiguration tokenConfiguration,
            LockoutConfiguration lockoutConfiguration,
            IRedisDataProvider redisDataProvider,
            IConfiguration configuration,
            ServiceBusUserStore users = null)
        {
            // if the ServiceBusUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            _users = users;

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _emailService = emailService;
            _clientConfiguration = clientConfiguration;
            _tokenProvider = tokenProvider;
            _tokenConfiguration = tokenConfiguration;
            _lockoutConfiguration = lockoutConfiguration;
            _lockoutService = new LockoutService(redisDataProvider, _lockoutConfiguration.LoginPeriod, _lockoutConfiguration.LoginAttempts, _lockoutConfiguration.LockLengthInMinutes);
            _configuration = configuration;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (_lockoutService.IsLockedOut(HttpContext.Connection.RemoteIpAddress.ToString()))
            {
                var errorViewModel = new ErrorViewModel
                {
                    Error = new ErrorMessage { Error = Resource.ErrorMessage_Blocked, ErrorDescription = Resource.ErrorMessage_BlockedIp }
                };
                return vm.IsMobileClient ? View("MobileError", errorViewModel) : View("Error", errorViewModel);
            }

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            if (vm.IsMobileClient)
            {
                return View("MobileLogin", vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            var isMobileClient = IsMobileClient(context?.ClientId);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // if the client is PKCE then we assume it's native, so this change in how to
                        // return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                // validate username/password against in-memory store
                if (await _users.ValidateCredentials(model.Username, model.Password))
                {
                    var user = await _users.FindByUsername(model.Username);

                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username));

                    // only set explicit expiration here if user chooses "remember me". 
                    // otherwise we rely upon expiration configured in cookie middleware.
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };

                    // issue authentication cookie with subject ID and username
                    await HttpContext.SignInAsync(user.SubjectId, user.Username, props);
                    _lockoutService.AccessSuccess(HttpContext.Connection.RemoteIpAddress.ToString());

                    if (context != null)
                    {
                        if (await _clientStore.IsPkceClientAsync(context.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(model.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);

                var failedAttempt = _lockoutService.AccessFailed(HttpContext.Connection.RemoteIpAddress.ToString());
                if (_lockoutService.IsLockedOut(HttpContext.Connection.RemoteIpAddress.ToString()))
                {
                    var errorViewModel = new ErrorViewModel
                    {
                        Error = new ErrorMessage {Error = Resource.ErrorMessage_Blocked, ErrorDescription = Resource.ErrorMessage_BlockedIp}
                    };
                    return isMobileClient ? View("MobileError", errorViewModel) : View("Error", errorViewModel);
                }
                if (failedAttempt > 1)
                {
                    ModelState.AddModelError(string.Empty, string.Format(Resource.ErrorMessage_LoginAttemptsLeft, _lockoutConfiguration.LoginAttempts - failedAttempt));
                }
            }
            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return isMobileClient ? View("MobileLogin", vm) : View(vm);
        }


        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        /// <summary>
        /// Entry point into the forgot password workflow
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword(bool isMobileClient, string returnUrl)
        {
            var vm = new ForgotPasswordViewModel {IsMobileClient = isMobileClient, ReturnUrl = returnUrl};
            if (isMobileClient)
            {
                return View("MobileForgotPassword", vm);
            }
            return View(vm);
        }

        /// <summary>
        /// Handle postback from forgot password
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _users.FindByUsername(model.Email);
                if (user == null)
                {
                    var errorMessage = model.IsMobileClient
                        ? Resource.ErrorMessage_MobileWrongEmail
                        : Resource.ErrorMessage_WrongEmail;
                    ModelState.AddModelError("Email", errorMessage);
                    var failedAttempt = _lockoutService.ResetPasswordFailed(HttpContext.Connection.RemoteIpAddress.ToString());
                    if (_lockoutService.IsLockedOut(HttpContext.Connection.RemoteIpAddress.ToString()))
                    {
                        return View(model.IsMobileClient ? "MobileError" : "Error",
                            new ErrorViewModel { Error = new ErrorMessage { Error = Resource.ErrorMessage_Blocked, ErrorDescription = Resource.ErrorMessage_BlockedIp } });
                    }
                    if (failedAttempt > 1)
                    {
                        ModelState.AddModelError(string.Empty, string.Format(Resource.ErrorMessage_LoginAttemptsLeft, _lockoutConfiguration.LoginAttempts - failedAttempt));
                    }

                    return model.IsMobileClient ? View("MobileForgotPassword", model) : View(model);
                }

                var token = GenerateSecurityToken(user);

                var callbackUrl = Url.Action("ResetPassword", "Account", new { token = token , isMobileClient = model.IsMobileClient, returnUrl = model.ReturnUrl}, protocol: Request.Scheme);
                var result = await _emailService.SendResetPasswordEmail(model.Email, callbackUrl, model.IsMobileClient);
                if (!result.Success)
                {
                    return View(model.IsMobileClient ? "MobileError" : "Error",
                        new ErrorViewModel { Error = new ErrorMessage { Error = Resource.ErrorMessage_Email, ErrorDescription = Resource.ErrorMessage_EmailSendingFailed } });
                }

                return model.IsMobileClient 
                    ? View("MobileEmailSent", new RedirectViewModel { RedirectUrl = model.ReturnUrl }) 
                    : View("EmailSent", new RedirectViewModel { RedirectUrl = _clientConfiguration.SpaUrl });
            }

            return model.IsMobileClient ? View("MobileForgotPassword", model) : View(model);
        }

        /// <summary>
        /// Entry point into the reset password workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, bool isMobileClient, string returnUrl)
        {
            var userId = await GetUserIdFromToken(token);
            if (userId == 0)
            {
                return View("Error", new ErrorViewModel { Error = new ErrorMessage { Error = Resource.ErrorMessage_Token, ErrorDescription = Resource.ErrorMessage_InvalidToken } });
            }
            if (!IsTokenDateValid(token))
            {
                return View("Error", new ErrorViewModel { Error = new ErrorMessage { Error = Resource.ErrorMessage_Token, ErrorDescription = Resource.ErrorMessage_TokenExpired } });
            }
            var vm = new ResetPasswordViewModel { UserId = (int)userId, IsMobileClient = isMobileClient, ReturnUrl = returnUrl};
            return View(vm);
        }

        /// <summary>
        /// Handle postback from reset password.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _users.ResetUserPassword(model.UserId, model.Password);
                if (!response.Success)
                {
                    return View("Error", new ErrorViewModel { Error = new ErrorMessage { Error = Resource.ErrorMessage_ResetFailed, ErrorDescription = Resource.ErrorMessage_ResetPasswordFailed } });
                }

                return View("PasswordReset", new RedirectViewModel { RedirectUrl = _clientConfiguration.SpaUrl, IsMobileClient = model.IsMobileClient });
            }

            return View(model);
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            var isMobileClient = false;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }

                    isMobileClient = IsMobileClient(client.ClientId);
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray(),
                IsMobileClient = isMobileClient
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri ?? _clientConfiguration.SpaUrl,   //ALOTE-884 When new version is deployed system doesn't redirect user to login page after logout
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private string GenerateSecurityToken(ApplicationUser user)
        {
            var claims = new Dictionary<string, object>();
            var expire = DateTime.UtcNow.Add(_tokenConfiguration.AccessTokenExpireTimeSpan);
            claims.Add("user", user.Username);
            claims.Add("expire", expire);
            return _tokenProvider.GenerateSecurityToken(claims);
        }

        private async Task<long> GetUserIdFromToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var claims = _tokenProvider.GetClaimsFromToken(token);
                if (claims != null && claims.TryGetValue("user", out object userName))
                {
                    var user = await _users.FindByUsername((string)userName);
                    if (user != null)
                    {
                        return user.Id;
                    }
                }
            }

            return 0;
        }

        private bool IsTokenDateValid(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var claims = _tokenProvider.GetClaimsFromToken(token);
                if (claims.TryGetValue("expire", out object expire))
                {
                    return DateTime.Compare(DateTime.UtcNow, (DateTime)expire) <= 0;
                }
            }

            return false;
        }

        private bool IsMobileClient(string clientId)
        {
            return string.Equals(clientId, "iam-mobile-client-app", StringComparison.OrdinalIgnoreCase);
        }
    }
}