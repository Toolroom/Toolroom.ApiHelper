using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public class JsonApiResourceIdentiferObject : JsonApiResource
    {
        public JsonApiResourceIdentiferObject(string id, string type)
        {
            Id = id;
            Type = type;
        }
        
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public object Metadata { get; set; }
    }
}