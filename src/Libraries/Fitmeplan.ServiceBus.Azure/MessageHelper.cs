using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace Fitmeplan.ServiceBus.Azure
{
    public static class MessageHelper
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static T DeserializeMsg<T>(this Message message) where T : class
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body), _settings);
        }
        public static Message AsMessage(this object obj)
        {
            return new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings)));
        }

        public static byte[] AsBody(this object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings));
        }
    }
}