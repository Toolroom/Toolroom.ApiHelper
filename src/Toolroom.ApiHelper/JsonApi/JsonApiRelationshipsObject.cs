using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Toolroom.ApiHelper
{
    public static class JsonApiRelationshipsObjectExtensions
    {
        public static int? GetRelationId(this JsonApiRelationshipsObject relationships, string relationKey)
        {
            if (relationships == null)
            {
                return null;
            }
            if (relationships.ContainsKey(relationKey))
            {
                var relation = relationships[relationKey] as JsonApiToOneRelationship;
                int id;
                if (relation != null && int.TryParse(relation.Data.Id, out id))
                {
                    return id;
                }
            }
            return null;
        }

        public static IEnumerable<int> GetRelationIds(this JsonApiRelationshipsObject relationships, string relationKey)
        {
            if (relationships == null)
            {
                yield break;
            }
            if (relationships.ContainsKey(relationKey))
            {
                var relations = relationships[relationKey] as JsonApiToManyRelationship;
                if (relations?.Data != null)
                {
                    foreach (var relation in relations.Data)
                    {
                        int id;
                        if (relation != null && int.TryParse(relation.Id, out id))
                        {
                            yield return id;
                        }
                    }
                }
            }
        }
    }

    public class JsonApiRelationshipsObject : Dictionary<string, JsonApiRelationshipBase>
    {

    }

    public class JsonApiRelationshipsObjectConverter : AbstractJsonConverter<JsonApiRelationshipBase>
    {
        protected override JsonApiRelationshipBase Create(Type objectType, JObject jObject)
        {
            if (FieldExists(jObject, "data", JTokenType.Object))
                return new JsonApiToOneRelationship();

            if (FieldExists(jObject, "data", JTokenType.Array))
                return new JsonApiToManyRelationship();

            //if (FieldExists(jObject, "data", JTokenType.Null))
            //    return null;

            throw new InvalidOperationException();
        }
    }

    public abstract class AbstractJsonConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (FieldExists(jObject, "data", JTokenType.Null)) return null;

            T target = Create(objectType, jObject);
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected static bool FieldExists(
            JObject jObject,
            string name,
            JTokenType type)
        {
            JToken token;
            return jObject.TryGetValue(name, out token) && token.Type == type;
        }
    }
}