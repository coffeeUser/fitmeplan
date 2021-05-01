using System.Collections.Generic;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.Api
{
    public class ResponseModel
    {
        public string Status { get; set; }
        public object Data { get; set; }
        public IList<ErrorInfo> Errors { get; set; }
        public string Message { get; set; }
    }
}
