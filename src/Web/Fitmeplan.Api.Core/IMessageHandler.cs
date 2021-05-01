using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api.Core
{
    public interface IMessageHandler : IOperationDescription
    {
        /// <summary>
        /// Invokes handler asynchronously.
        /// </summary>
        /// <param name="payload">The request.</param>
        /// <returns></returns>
        Task<ResponseMessage> InvokeAsync(object payload);

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="evt">The evt.</param>
        /// <returns></returns>
        Task HandleEventAsync(JObject evt);
    }
}