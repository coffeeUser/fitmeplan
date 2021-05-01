using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Fitmeplan.Api.Core.DictionaryHandlers
{
    public interface ILookupHandler
    {
        /// <summary>
        /// Gets the name of the dictionary.
        /// </summary>
        string LookupName { get; }

        /// <summary>
        /// Invokes the asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        Task<object> InvokeAsync(JObject model);
    }
}