using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public abstract class JsonBaseModel
    {
        [JsonIgnore]
        public int Id { get; set; }
    }
}
