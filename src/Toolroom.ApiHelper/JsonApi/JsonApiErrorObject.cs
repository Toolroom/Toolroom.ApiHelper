using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public class JsonApiErrorObject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiErrorLinkObject Links { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Source { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }

    }
}