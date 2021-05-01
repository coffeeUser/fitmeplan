using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Fitmeplan.ServiceBus.Core;

namespace Fitmeplan.IdentityServer
{
    public static class HttpContextExtensions
    {
        private static readonly string AcceptLanguageHeader = "accept-language";
        private static readonly string DefaultCulture = "en-us";
        public static string Culture(this HttpContext httpContext)
        {
            return httpContext.Request?.Headers.ContainsKey(AcceptLanguageHeader) ?? false ? 
                httpContext.Request.Headers[AcceptLanguageHeader].First().ToLowerInvariant() :
                DefaultCulture;
        }

        public static CorrelationContext GetBusContext(this HttpContext httpContext)
        {
            var activityId = Trace.CorrelationManager.ActivityId;
            var origin = httpContext.Request.GetDisplayUrl();
            var culture = httpContext.Culture();
            return (CorrelationContext) CorrelationContext.Create<ICommand>(activityId, httpContext.User, Guid.Empty, origin, null, string.Empty);
        }
    }
}
