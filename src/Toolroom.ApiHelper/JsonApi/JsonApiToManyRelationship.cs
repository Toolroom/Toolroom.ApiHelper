using System.Collections.Generic;
using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public class JsonApiToManyRelationship : JsonApiRelationshipBase
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<JsonApiResourceIdentiferObject> Data
        {
            get
            {
                return RawData as IEnumerable<JsonApiResourceIdentiferObject>;
            }

            set
            {
                RawData = value;
            }
        }
    }
}