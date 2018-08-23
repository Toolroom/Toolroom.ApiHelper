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
            foreach (JObject jObject in jArray.Children<JObject>())
            {
                string type = jObject.Value<string>("type");
                string id = jObject.Value<string>("id");
                JObject attr = jObject.Value<JObject>("attributes");
                string json = attr.ToString(Formatting.None);
                System.Diagnostics.Debug.WriteLine(json);
                target.Add(new JsonApiResourceObject<string>(id, type, json));
            }

            return target;
        }
    }

}