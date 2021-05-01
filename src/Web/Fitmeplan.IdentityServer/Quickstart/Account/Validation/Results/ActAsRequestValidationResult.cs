using System.Collections.Generic;
using IdentityServer4.Validation;

namespace Fitmeplan.IdentityServer.Quickstart.Account.Validation.Results
{
    public class ActAsRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActAsRequestValidationResult"/> class.
        /// </summary>
        /// <param name="validatedRequest">The validated request.</param>
        /// <param name="customResponse">The custom response.</param>
        public ActAsRequestValidationResult(ValidatedActAsRequest validatedRequest, Dictionary<string, object> customResponse = null)
        {
            IsError = false;

            ValidatedRequest = validatedRequest;
            CustomResponse = customResponse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActAsRequestValidationResult"/> class.
        /// </summary>
        /// <param name="validatedRequest">The validated request.</param>
        /// <param name="error">The error.</param>
        /// <param name="errorDescription">The error description.</param>
        /// <param name="customResponse">The custom response.</param>
        public ActAsRequestValidationResult(ValidatedActAsRequest validatedRequest, string error, string errorDescription = null, Dictionary<string, object> customResponse = null)
        {
            IsError = true;
            Error = error;
            ErrorDescription = errorDescription;
            ValidatedRequest = validatedRequest;
            CustomResponse = customResponse;
        }

        /// <summary>
        /// Gets the validated request.
        /// </summary>
        /// <value>
        /// The validated request.
        /// </value>
        public ValidatedActAsRequest ValidatedRequest { get; }

        /// <summary>
        /// Gets or sets the custom response.
        /// </summary>
        /// <value>
        /// The custom response.
        /// </value>
        public Dictionary<string, object> CustomResponse { get; set; }
    }
}
