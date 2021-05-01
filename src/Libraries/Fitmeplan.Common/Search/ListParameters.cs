using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fitmeplan.Common.Search
{
    public class ListParameters
    {
        public PagingOptions Paging { get; set; }
        [JsonConverter(typeof(FilterConverter))]
        public List<FilterBase> Filter { get; set; }
        public string SearchString { get; set; }
    }
}
