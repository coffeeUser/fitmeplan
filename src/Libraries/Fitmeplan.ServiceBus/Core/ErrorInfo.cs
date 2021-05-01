using System;

namespace Fitmeplan.ServiceBus.Core
{
    /// <summary>
    /// Iformation about error
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Default constructor of class
        /// </summary>
        public ErrorInfo()
            : this(string.Empty, string.Empty)
        {
        }

        public ErrorInfo(string key, string errorMessage)
        {
            Key = key;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="key">Key of error (for example, property name)</param>
        /// <param name="message">Error message</param>
        /// <param name="messageParams">Error message params</param>
        public ErrorInfo(string key, string message, params object[] messageParams)
            : this(key ?? Guid.NewGuid().ToString(), message)
        {
            Params = messageParams;
        }

        /// <summary>
        /// Key of error
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error message params
        /// </summary>
        public object[] Params { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}. Key: '{Key}', ErrorMessage: '{ErrorMessage}'";
        }
    }
}