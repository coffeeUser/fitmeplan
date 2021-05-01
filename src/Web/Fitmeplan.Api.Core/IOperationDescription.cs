using System;

namespace Fitmeplan.Api.Core
{
    public interface IOperationDescription
    {
        /// <summary>
        /// Gets the name of the operation.
        /// </summary>
        /// <value>
        /// The name of the operation.
        /// </value>
        string OperationName { get; }

        /// <summary>
        /// Gets the type of the operation.
        /// </summary>
        /// <value>
        /// The type of the operation.
        /// </value>
        Type OperationType { get; }

        /// <summary>
        /// Gets the type of the response.
        /// </summary>
        Type ResponseType { get; }

        /// <summary>
        /// Gets/sets the mobile operation identifier flag.
        /// </summary>
        bool IsMobileOperation { get; set; }

        /// <summary>
        /// Gets/sets the hybrid operation identifier flag.
        /// </summary>
        bool IsHybridOperation { get; set; }

        /// <summary>
        /// Determines whether [is opeation name parameter] [the specified parameter name].
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>
        ///   <c>true</c> if [is opeation name parameter] [the specified parameter name]; otherwise, <c>false</c>.
        /// </returns>
        bool IsOpeationNameParameter(string paramName);
    }
}