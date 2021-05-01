using System;
using System.Collections.Generic;
using System.Linq;

namespace Fitmeplan.Api.Core
{
    public class MessageHandlerProvider
    {
        private readonly Dictionary<string, IMessageHandler> _registry;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public MessageHandlerProvider(IEnumerable<IMessageHandler> handlers)
        {
            _registry = handlers.Where(x => !x.IsMobileOperation || x.IsHybridOperation).ToDictionary(x => x.OperationName.ToLower(), y => y);
        }


        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public IMessageHandler GetHandler(string messageType)
        {
            var caseInsensitiveType = messageType.ToLower();
            if (_registry.ContainsKey(caseInsensitiveType))
            {
                return _registry[caseInsensitiveType];
            }

            throw new NotSupportedException(messageType);
        }
    }
}
