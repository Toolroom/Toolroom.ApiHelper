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
            throw new NotImplementedException();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JArray jArray = JArray.Load(reader);

            JsonApiInclusionCollection target = new JsonApiInclusionCollection();

            foreach (var jToken in jArray)
            {
                //get class type name
                string classname = jToken.Value<string>("type");
                string id = jToken.Value<string>("id");
                //infer type
                Type targetType = JsonApiDocument.QueryType(classname);
                var instance = jToken.ToObject(targetType, serializer) as JsonBaseModel;
                if (instance == null) continue;
                target.Add(new JsonApiResourceObject<JsonBaseModel>(id, classname, instance));
            }

            return target;
        }
    }

}