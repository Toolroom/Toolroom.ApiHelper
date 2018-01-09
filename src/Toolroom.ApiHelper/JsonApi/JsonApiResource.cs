using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public abstract class JsonApiResource
    {
        public string Id { get; set; }
        public string Type { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; }
    }
}