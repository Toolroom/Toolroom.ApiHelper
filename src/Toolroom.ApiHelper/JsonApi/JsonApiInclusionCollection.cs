using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toolroom.ApiHelper
{
    [JsonConverter(typeof(JsonApiInclusionCollectionConverter))]
    public class JsonApiInclusionCollection : List<JsonApiResourceObject>
    {

    }


    public class JsonApiInclusionCollectionConverter : AbstractJsonConverter<JsonApiInclusionCollection>
    {
        protected override JsonApiInclusionCollection Create(Type objectType, JObject jObject)
        {
            return new JsonApiInclusionCollection();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JArray jArray = JArray.Load(reader);

            JsonApiInclusionCollection target = Create(null,null);

            foreach (var jToken in jArray)
            {
                //get class type name
                string classname = jToken.Value<string>("type");
                string id = jToken.Value<string>("id");
                if (string.IsNullOrEmpty(classname) || string.IsNullOrEmpty("id")) continue;
                
                var attr = jToken["attributes"];
                
                //infer type
                Type targetType = JsonApiDocument.QueryType(classname);

                if(targetType != null)
                {
                    var instance = attr.ToObject(targetType, serializer);
                    if (instance == null) continue;
                    target.Add(new JsonApiResourceObject<JsonBaseModel>(id, classname, instance as JsonBaseModel));
                }
                else
                {
                    target.Add(new JsonApiResourceObject<object>(id, classname, attr.ToObject(typeof( object ))));
                }
            }

            return target;
        }
    }

}