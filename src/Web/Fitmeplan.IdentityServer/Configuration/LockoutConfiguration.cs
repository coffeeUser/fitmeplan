using System;

namespace Fitmeplan.IdentityServer.Configuration
{
    public class LockoutConfiguration
    {
        public TimeSpan LoginPeriod { get; set; }
        public int LoginAttempts { get; set; }
        public int LockLengthInMinutes { get; set; }
    }
}
