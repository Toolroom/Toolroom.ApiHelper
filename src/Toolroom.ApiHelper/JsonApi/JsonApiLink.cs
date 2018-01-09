using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public class JsonApiLink
    {
        public JsonApiLink(string url, object metadata = null)
        {
            Url = url;
            Metadata = metadata;
        }

        [JsonProperty("href")]
        public string Url { get; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public object Metadata { get; }
    }
}