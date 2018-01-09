using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public class JsonApiLinksObject
    {
        public JsonApiLinksObject(JsonApiLink self = null, JsonApiLink related = null)
        {
            Self = self;
            Related = related;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Self { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Related { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Prev { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Next { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink First { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Last { get; set; }
    }
}