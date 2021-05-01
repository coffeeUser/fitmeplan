using System;

namespace Fitmeplan.Api.Core.Security
{
    public class NotAuthorizedException : Exception
    {
        /// <summary>
        /// Gets the name of the operation.
        /// </summary>
        public string OperationName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAuthorizedException"/> class.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        public NotAuthorizedException(string operationName)
        {
            OperationName = operationName;
        }
    }
}