using System;
using System.Collections.Generic;
using System.Text;

namespace Fitmeplan.ServiceBus
{
    public class ServiceBusConfiguration
    {
        public string Transport { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the subscriber queue prefix.
        /// </summary>
        public string ApplicationName { get; set; }

        public int RequestTimeoutInSeconds { get; set; } = 60;
    }
}
