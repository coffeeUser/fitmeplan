using System.Globalization;
using System.Security.Claims;
using System.Threading;

namespace Fitmeplan.ServiceBus.Core
{
    public class BusContext
    {
        private readonly AsyncLocal<ICorrelationContext> _current = new AsyncLocal<ICorrelationContext>();
        private readonly AsyncLocal<ClaimsPrincipal> _claimsPrincipal = new AsyncLocal<ClaimsPrincipal>();
        private readonly AsyncLocal<CultureInfo> _cultureInfo = new AsyncLocal<CultureInfo>();

        private static BusContext _instance;

        protected static BusContext Instance
        {
            get
            {
                var context = _instance;
                if (context != null)
                {
                    return context;
                }
                return (_instance = new BusContext());
            }
        }

        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        public static ICorrelationContext Current
        {
            get => Instance._current.Value;
            set => Instance._current.Value = value;
        }

        /// <summary>
        /// Gets or sets the claims principal.
        /// </summary>
        public static ClaimsPrincipal ClaimsPrincipal
        {
            get => Instance._claimsPrincipal.Value;
            set => Instance._claimsPrincipal.Value = value;
        }

        /// <summary>
        /// Gets or sets the culture information.
        /// </summary>
        public static CultureInfo CultureInfo
        {
            get => Instance._cultureInfo.Value;
            set => Instance._cultureInfo.Value = value;
        }
    }
}
