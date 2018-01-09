using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public class JsonApiToOneRelationship : JsonApiRelationshipBase
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiResourceIdentiferObject Data {
            get
            {
                return (RawData as List<JsonApiResourceIdentiferObject>)?.FirstOrDefault();
            }

            set
            {
                RawData = new List<JsonApiResourceIdentiferObject> { value };
            }
        } 
    }
}