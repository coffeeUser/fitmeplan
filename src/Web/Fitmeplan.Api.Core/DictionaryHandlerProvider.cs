using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Fitmeplan.Api.Core.DictionaryHandlers;

namespace Fitmeplan.Api.Core
{
    public class DictionaryHandlerProvider
    {
        private readonly Dictionary<string, ILookupHandler> _registry;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public DictionaryHandlerProvider(IEnumerable<ILookupHandler> handlers)
        {
            _registry = handlers.ToDictionary(x => x.LookupName.ToLower(), y => y);
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="lookupType">Type of the message.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public ILookupHandler GetHandler(string lookupType)
        {
            var caseInsensitiveType = lookupType.ToLower();
            if (_registry.ContainsKey(caseInsensitiveType))
            {
                return _registry[caseInsensitiveType];
            }

            throw new NotSupportedException(lookupType);
        }

        public async Task<object> InvokeAsync(string lookupName, JObject model)
        {
            if ("index".Equals(lookupName, StringComparison.OrdinalIgnoreCase))
            {
                return _registry.Keys.OrderBy(x => x);
            }
            return await GetHandler(lookupName).InvokeAsync(model);
        }
    }
}
