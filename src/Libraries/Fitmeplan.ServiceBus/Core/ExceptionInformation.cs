using System;

namespace Fitmeplan.ServiceBus.Core
{
    /// <summary>
    /// Holds information about exception thrown in a remote message handler. 
    /// </summary>
    public class ExceptionInformation
    {
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
        public string InnerMessage { get; set; }
    }

    public static class ExceptionExtensions
    {
        public static Exception UnwrapInnerException(this Exception exception)
        {
            if (exception is AggregateException && exception.InnerException != null)
            {
                return UnwrapInnerException(exception.InnerException);
            }
            return exception;
        }
    }
}
