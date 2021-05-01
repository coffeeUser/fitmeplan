using System;
using System.Globalization;
using System.Threading;
using JWT;

namespace Fitmeplan.ServiceBus.Core
{
    /// <summary>
    /// Base class for service bus clients
    /// </summary>
    public abstract class ServiceBusClientBase
    {
        protected ServiceBusConfiguration Settings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusClientBase" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        protected ServiceBusClientBase(ServiceBusConfiguration settings)
        {
            Settings = settings;
        }

        protected void RestoreCurrentPrincipal(ICorrelationContext context)
        {
            try
            {
                Thread.CurrentPrincipal = context?.ClaimsPrincipal;
                var culture = context?.Culture ?? "en-GB";
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            }
            catch (TokenExpiredException)
            {
                Console.WriteLine("Token has expired");
            }
            catch (SignatureVerificationException)
            {
                Console.WriteLine("Token has invalid signature");
            }
        }
    }
}