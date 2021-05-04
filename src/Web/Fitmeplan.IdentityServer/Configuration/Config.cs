// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4.Models;

namespace Fitmeplan.IdentityServer.Configuration
{
    public static class Config
    {
        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="clientsUrl">The clients URL.</param>
        /// <param name="identityTokenLifetime">The identity token lifetime.</param>
        /// <param name="accessTokenLifetime">The access token lifetime.</param>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientsUrl, int identityTokenLifetime, int accessTokenLifetime, int mobileAccessTokenLifetime)
        {
            var sha256 = "secret".Sha256();
            return new[]
            {
                // fitmeplan-client-app
                new Client
                {
                    ClientId = "fitmeplan-client-app",
                    ClientName = "Fitmeplan SPA Client",
            
                    ClientSecrets =
                    {
                        new Secret(sha256)
                    },
                    AllowedGrantTypes = new[] { "hybrid", "client_credentials", "refresh_token" },
                    AllowedScopes = new[] { "openid", "profile", "api", "email" },
                    AllowOfflineAccess = true,

                    RedirectUris = new[] { $"{clientsUrl["SpaBff"]}/signin-oidc" },
                    FrontChannelLogoutUri = $"{clientsUrl["SpaBff"]}/signout-oidc",
                    PostLogoutRedirectUris = new[] { $"{clientsUrl["SpaBff"]}/signout-callback-oidc" },
                    RequireConsent = false, 
                    AllowedCorsOrigins = new List<string>
                    {
                        clientsUrl["SpaBff"]
                    },
                    IdentityTokenLifetime = identityTokenLifetime, // Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
                    AccessTokenLifetime = accessTokenLifetime, // Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
                },

                // swagger-client
                new Client
                {
                    ClientId="swagger-client",
                    ClientName="swagger-client",

                    AllowedGrantTypes = new[] { "implicit" },
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = new[] { $"{clientsUrl["Swagger"]}/swagger/oauth2-redirect.html" },
                    AllowedScopes = new[] { "openid", "profile", "api", "email" },
                    RequireConsent= false ,
                    IdentityTokenLifetime = identityTokenLifetime, // Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
                    AccessTokenLifetime = accessTokenLifetime, // Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
                },

                //fitmeplan-mobile-client-app
                new Client
                {
                    ClientId = "fitmeplan-mobile-client-app",
                    ClientName = "Fitmeplan Mobile Client",
                    AllowedGrantTypes = new[] { "authorization_code", "client_credentials", "refresh_token" },
                    RequirePkce = false,
                    RequireConsent = false,
                    ClientSecrets =
                    {
                        new Secret(sha256)
                    },
                    RedirectUris = new[] { "myapp://callback/" },
                    AllowedScopes = new[] { "openid", "profile", "api", "email" },
                    AllowOfflineAccess = true,
                    IdentityTokenLifetime = identityTokenLifetime, // Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
                    AccessTokenLifetime = mobileAccessTokenLifetime, // Lifetime of mobile access token in seconds (defaults to 86400 seconds / 24 hour)
                }
            };
        }
    }
}