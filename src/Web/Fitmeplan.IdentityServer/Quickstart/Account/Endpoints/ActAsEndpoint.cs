using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using NLog;
using Fitmeplan.Identity;
using Fitmeplan.IdentityServer.Quickstart.Account.ResponseHandling;
using Fitmeplan.IdentityServer.Quickstart.Account.Results;
using Fitmeplan.IdentityServer.Quickstart.Account.Validation;

namespace Fitmeplan.IdentityServer.Quickstart.Account.Endpoints
{
    public class ActAsEndpoint : IEndpointHandler
    {
        private readonly IClientSecretValidator _clientValidator;
        private readonly IActAsRequestValidator _requestValidator;
        private readonly IActAsResponseGenerator _responseGenerator;
        private readonly ServiceBusUserStore _userStore;
        private readonly Logger _logger;

        public ActAsEndpoint(IClientSecretValidator clientValidator,
                            IActAsRequestValidator requestValidator,
                            IActAsResponseGenerator responseGenerator,
                            ServiceBusUserStore userStore)
        {
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _responseGenerator = responseGenerator;
            _logger = LogManager.GetLogger(nameof(ActAsEndpoint));
            _userStore = userStore;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.Info("Processing act as request.");

            // validate HTTP
            if (!HttpMethods.IsPost(context.Request.Method) || !context.Request.HasFormContentType)
            {
                _logger.Warn("Invalid HTTP request for act as endpoint");
                return Error(OidcConstants.TokenErrors.InvalidRequest);
            }

            _logger.Info("Start act as request.");

            // validate client
            var clientValidationResult = await _clientValidator.ValidateAsync(context);

            if (clientValidationResult.Client == null)
            {
                return Error(OidcConstants.TokenErrors.InvalidClient);
            }

            // validate request
            var form = (await context.Request.ReadFormAsync()).AsNameValueCollection();

            var validationRequestResult = await _requestValidator.ValidateRequestAsync(form, clientValidationResult);

            if (validationRequestResult.IsError)
            {
                return Error(validationRequestResult.Error,
                    validationRequestResult.ErrorDescription,
                    validationRequestResult.CustomResponse);
            }

            // validate user
            var subId = validationRequestResult.ValidatedRequest.PrincipalId.Substring(4);
            var user = await _userStore.FindBySubjectId(subId);
            if (user == null)
            {
                return Error(OidcConstants.AuthorizeErrors.InvalidRequestObject);
            }

            validationRequestResult.ValidatedRequest.ClientClaims = user.GetClaims();

            var response = await _responseGenerator.ProcessAsync(validationRequestResult);

            // return result
            _logger.Info("ActAs request success.");
            return new ActAsResult(response);
        }

        private EndpointErrorResult Error(string error, string errorDescription = null, Dictionary<string, object> custom = null)
        {
            var response = new TokenErrorResponse
            {
                Error = error,
                ErrorDescription = errorDescription,
                Custom = custom
            };

            return new EndpointErrorResult(response);
        }
    }
}
