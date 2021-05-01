using System;

namespace Fitmeplan.IdentityServer.Configuration
{
    public class TokenConfiguration
    {
        public string Secret { get; set; }
        public TimeSpan AccessTokenExpireTimeSpan { get; set; }
    }
}
