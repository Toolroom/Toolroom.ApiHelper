using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    [JsonConverter(typeof(JsonApiRelationshipsObjectConverter))]
    public abstract class JsonApiRelationshipBase
    {
        [JsonIgnore]
        protected object RawData { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }
    }
}