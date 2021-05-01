using System;

namespace Fitmeplan.ServiceBus.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageSettingsAttribute : Attribute
    {
        public string Name { get; set; }

        public bool Durable { get; set; }

        public bool AutoDelete { get; set; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Attribute"></see> class.</summary>
        public MessageSettingsAttribute(string name = null, bool durable = false, bool autoDelete = false)
        {
            Name = name;
            Durable = durable;
            AutoDelete = autoDelete;
        }
    }
}
