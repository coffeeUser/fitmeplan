using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fitmeplan.Common.Search
{
    public class FilterConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Not implemented yet");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);
                var list = array.Select(x => ResolveFilterExpression((JObject) x)).Where(x => x != null).ToList();
                return list;
            }
            return new List<FilterBase>();
        }

        private static FilterBase ResolveFilterExpression(JObject item)
        {
            if (CheckType(item, FilterType.Equal))
            {
                return item.ToObject<EqualFilter>();
            }
            if (CheckType(item, FilterType.Date))
            {
                return item.ToObject<DateFilter>();
            }
            if (CheckType(item, FilterType.RangeFilter))
            {
                return item.ToObject<RangeFilter>();
            }
            if (CheckType(item, FilterType.ContainsText))
            {
                return item.ToObject<ContainsTextFilter>();
            }
            if (CheckType(item, FilterType.In))
            {
                return item.ToObject<InFilter>();
            }
            if (CheckType(item, FilterType.Hierarchy))
            {
                return item.ToObject<HierarchyFilter>();
            }
            return null;
        }

        private static bool CheckType(JObject item, string type)
        {
            return type.Equals(item.GetValue("type", StringComparison.OrdinalIgnoreCase)?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }
}