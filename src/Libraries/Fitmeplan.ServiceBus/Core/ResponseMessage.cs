using System.Collections.Generic;

namespace Fitmeplan.ServiceBus.Core
{
    /// <summary>
    /// Represents result of an action.
    /// </summary>
    public class ResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMessage"/> class.
        /// </summary>
        public ResponseMessage() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMessage"/> class.
        /// </summary>
        public ResponseMessage(object result)
        {
            Errors = new List<ErrorInfo>();
            Data = result;
        }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public ExceptionInformation Exception { get; set; }

        private bool? _success;
        /// <summary>
        ///     Indicates if result is successful.
        /// </summary>
        public bool Success
        {
            get { return _success ?? Errors.Count == 0 && Exception == null; }
            set { _success = value; }
        }

        /// <summary>
        /// 	Gets list of errors.
        /// </summary>
        public IList<ErrorInfo> Errors { get; private set; }

        public object Data { get; set; }

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="message">The message.</param>
        public void AddError(string key, string message)
        {
            Errors.Add(new ErrorInfo { Key = key, ErrorMessage = message });
        }
    }
}