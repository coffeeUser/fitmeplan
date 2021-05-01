using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;
using NLog;
using Fitmeplan.IdentityServer.Quickstart.Account.Validation.Results;

namespace Fitmeplan.IdentityServer.Quickstart.Account.Validation
{
    public class ActAsRequestValidator : IActAsRequestValidator
    {
        private readonly IdentityServerOptions _options;
        private readonly ITokenValidator _tokenValidator;
        private readonly Logger _logger;

        private ValidatedActAsRequest _validatedRequest;

        public ActAsRequestValidator(IdentityServerOptions options,
            ITokenValidator tokenValidator)
        {
            _options = options;
            _tokenValidator = tokenValidator;
            _logger = LogManager.GetLogger(nameof(ActAsRequestValidator));
        }

        public async Task<ActAsRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult)
        {
            _validatedRequest = new ValidatedActAsRequest
            {
                Raw = parameters ?? throw new ArgumentNullException(nameof(parameters)),
                Options = _options
            };

            if (clientValidationResult == null) throw new ArgumentNullException(nameof(clientValidationResult));

            _validatedRequest.SetClient(clientValidationResult.Client, clientValidationResult.Secret);


            // check client protocol type
            if (_validatedRequest.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.OpenIdConnect)
            {
                _logger.Error("Invalid protocol type for client",
                    new
                    {
                        clientId = _validatedRequest.Client.ClientId,
                        expectedProtocolType = IdentityServerConstants.ProtocolTypes.OpenIdConnect,
                        actualProtocolType = _validatedRequest.Client.ProtocolType
                    });

                return Invalid(OidcConstants.TokenErrors.InvalidClient);
            }

            // check access token
            var accessToken = parameters.Get("access_token");
            if (IEnumerableExtensions.IsNullOrEmpty(accessToken))
            {
                return Invalid("invalid_access_token");
            }

            var accessTokenValidationResult = await _tokenValidator.ValidateAccessTokenAsync(accessToken);
            if (accessTokenValidationResult.IsError)
            {
                return Invalid("invalid_access_token");
            }
            else
            {
                _validatedRequest.AccessToken = accessTokenValidationResult.Jwt;
            }

            // check principal Id
            var principalIdString = parameters.Get("principal_id");
            if (IEnumerableExtensions.IsNullOrEmpty(principalIdString))
            {
                return Invalid("invalid_principal_id");
            }

            _validatedRequest.PrincipalId = principalIdString;
            _validatedRequest.Scopes = parameters.Get(OidcConstants.TokenRequest.Scope).Split(' ').ToList();

            return Valid();
        }

        private ActAsRequestValidationResult Valid(Dictionary<string, object> customResponse = null)
        {
            return new ActAsRequestValidationResult(_validatedRequest, customResponse);
        }

        private ActAsRequestValidationResult Invalid(string error, string errorDescription = null, Dictionary<string, object> customResponse = null)
        {
            return new ActAsRequestValidationResult(_validatedRequest, error, errorDescription, customResponse);
        }
    }
}
