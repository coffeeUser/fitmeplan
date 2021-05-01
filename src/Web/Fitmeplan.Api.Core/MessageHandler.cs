using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Fitmeplan.Api.Core.Security;
using Fitmeplan.Common;
using Fitmeplan.Contracts.Requests;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api.Core
{
    public class MessageHandler<TIn, TOut> : IMessageHandler where TIn : RequestBase
    {
        private readonly AuthorizationService _authorizationService;

        /// <summary>
        /// Gets the bus.
        /// </summary>
        public IServiceBus Bus { get; }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string OperationName => typeof(TIn).Name;

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        public Type OperationType => typeof(TIn);

        /// <summary>
        /// Gets the type of the response.
        /// </summary>
        public Type ResponseType => typeof(TOut);

        /// <summary>
        /// Gets/sets the mobile handler identifier flag.
        /// </summary>
        public bool IsMobileOperation { get; set; }

        /// <summary>
        /// Gets/sets the hybrid handler identifier flag.
        /// </summary>
        public bool IsHybridOperation { get; set; }

        /// <summary>
        /// Determines whether [is message type parameter] [the specified parameter name].
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>
        ///   <c>true</c> if [is message type parameter] [the specified parameter name]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOpeationNameParameter(string paramName)
        {
            return paramName.Equals("queryType", StringComparison.OrdinalIgnoreCase)
                   || paramName.Equals("commandType", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"></see> class.
        /// </summary>
        /// <param name="serviceBus">The service bus.</param>
        /// <param name="authorizationService">The authorization service.</param>
        public MessageHandler(IServiceBus serviceBus, AuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            Bus = serviceBus;
        }

        /// <summary>
        /// Invokes handler asynchronously.
        /// </summary>
        /// <param name="payload">The request.</param>
        /// <returns></returns>
        public async Task<ResponseMessage> InvokeAsync(object payload)
        {
            TIn input;
            if (payload is JContainer)
            {
                input = ((JContainer)payload).ToObject<TIn>();
            }
            else
            {
                input = await CreateRequestAsync(payload);
            }
            return await HandleRequestAsync(input);
        }

        protected virtual Task<TIn> CreateRequestAsync(object payload)
        {
            return Task.FromResult((TIn) payload);
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public virtual async Task<ResponseMessage> HandleRequestAsync(TIn request)
        {
            var data = (request as dynamic).Data as object;

            var res = await _authorizationService.AuthorizeAsync(OperationName);

            if (!res.Succeeded)
            {
                throw new NotAuthorizedException(OperationName);
            }

            if (data != null && !TryValidate(data, out var result))
            {
                var responseMessage = new ResponseMessage();
                foreach (var validationResult in result)
                {
                    responseMessage.AddError(validationResult.MemberNames.First().ToCamelCase(), validationResult.ErrorMessage);
                }
                return responseMessage;
            }
            return await Bus.RequestAsync<TIn, ResponseMessage>(request);
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="evt">The evt.</param>
        /// <returns></returns>
        public Task HandleEventAsync(JObject evt)
        {
            var result = evt.ToObject<TIn>();
            return HandleEventAsync(result);
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="evt">The evt.</param>
        /// <returns></returns>
        public virtual Task HandleEventAsync(TIn evt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries the validate.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        public virtual bool TryValidate(object @object, out ICollection<ValidationResult> results)
        {
            var context = new ValidationContext(@object, serviceProvider: null, items: null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(@object, context, results, validateAllProperties: true);
        }
    }
}