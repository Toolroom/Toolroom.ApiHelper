using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{

    public class JsonApiDocument
    {
        [JsonIgnore]
        public object RawData { get; protected set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; protected set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiErrors Errors { get; protected set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; protected set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiInclusionCollection Included { get; protected set; }

        public void AddError(string uid, JsonApiLink aboutLink, int httpStatusCode, string appErrorCode, string title, string detail, object source, object meta)
        {
            if (Errors == null)
            {
                Errors = new JsonApiErrors();
            }

            Errors.Add(uid, aboutLink, httpStatusCode, appErrorCode, title, detail, source, meta);
        }

        public IEnumerable<JsonApiResourceObject<T>> ExtractFromIncludes<T>() where T : JsonBaseModel
        {
            // create container for storing the result set
            List<JsonApiResourceObject<T>> results = new List<JsonApiResourceObject<T>>();

            // gather meta info
            Type modeltype = typeof(T);
            var classattribs = modeltype.GetCustomAttributes(typeof(JsonClassAttribute), false);
            string modelName = (classattribs?.FirstOrDefault() as JsonClassAttribute)?.Name;

            //find relevant attributes in includes
            var data = Included.Where(x => x.Type.Equals(modelName));

            //try to convert the included attributes to datamodels
            foreach(var resource in data)
            {
                string jsonString = resource.Attributes as string;
                JsonApiResourceObject<T> model = null;
                try
                {
                    model = Newtonsoft.Json.Linq.JObject.Parse(jsonString)?.ToObject<JsonApiResourceObject<T>>();
                }
                catch (Exception e)
                {
                }
                if (model == null) continue;
                //int id = -1;
                //int.TryParse(resource.Id, out id);
                //model.Id = id;
                //var resourceT = new JsonApiResourceObject<T>(resource.Id, resource.Type, model);
                //resourceT.Links = resource.Links;
                //resourceT.Relationships = resource.Relationships;
                results.Add(model);
            }

            return results;
        }
    }

    public class JsonApiDocument<T> : JsonApiDocument where T : JsonBaseModel
    {
        #region Constructors

        public JsonApiDocument()
        {

        }

        public JsonApiDocument(T data, JsonApiErrors errors, object metadata = null, JsonApiLinksObject links = null, JsonApiRelationshipsObject relationships = null, JsonApiInclusionCollection includedData = null)
        {
            Func<T, string> idResolver = d => (d as ICustomJsonModelId)?.CustomJsonModelId ?? d.Id.ToString();

            Meta = metadata;
            Errors = errors;
            if (data == null)
            {
                Data = null;
                Included = null;
            }
            else
            {
                JsonClassAttribute classAttrib = data.GetType().GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
                if (classAttrib == null) { throw new Exception("No class attribute specified for " + data.GetType().Name + "."); }
                Data = new JsonApiResourceObject<T>(idResolver(data), classAttrib.Name, data)
                {
                    Relationships = relationships
                };

                Included = includedData;
            }

            Links = links;
        }

        public JsonApiDocument(JsonApiErrors errors, object meta = null, JsonApiLinksObject links = null)
        {
            Meta = meta;
            Errors = errors;
            Data = null;
            Included = null;
            Links = links;
        }

        #endregion

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public JsonApiResourceObject<T> Data
        {
            get
            {
                return (RawData as List<JsonApiResourceObject<T>>)?.FirstOrDefault();
            }

            set
            {
                RawData = new List<JsonApiResourceObject<T>> { value };
            }
        }

        #region 1:1

        public void AddOneToOneRelation<TRelatedObject>(TRelatedObject relatedObject)
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToOneRelation(this, relatedObject);
        }

        public void AddOneToOneRelation<TRelatedObject>(string relatedObjectKey, TRelatedObject relatedObject)
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToOneRelation(this, relatedObjectKey, relatedObject);
        }

        public void AddOneToOneRelation<TBaseObject, TRelatedObject>(TBaseObject baseObject, TRelatedObject relatedObject)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToOneRelation(this, baseObject, relatedObject);
        }

        public void AddOneToOneRelation<TBaseObject, TRelatedObject>(TBaseObject baseObject, string relatedObjectKey, TRelatedObject relatedObject)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToOneRelation(this, baseObject, relatedObjectKey, relatedObject);
        }

        public void AddOneToOneRelationOnly<TRelatedObject>(string relatedObjectKey, string relatedObjectId)
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToOneRelationOnly<TRelatedObject>(this, relatedObjectKey, relatedObjectId);
        }

        public void AddOneToOneRelationOnly<TBaseObject, TRelatedObject>(TBaseObject baseObject, string relatedObjectKey, string relatedObjectId)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToOneRelationOnly<TBaseObject, TRelatedObject>(this, baseObject, relatedObjectKey, relatedObjectId);
        }

        #endregion

        #region 1:n

        public void AddOneToManyRelation<TBaseObject, TRelatedObject>(
            TBaseObject baseObject,
            string relatedObjectsKey, IEnumerable<TRelatedObject> relatedObjects,
            Uri selfUri = null, Uri relatedUri = null)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToManyRelation(this, baseObject, relatedObjectsKey, relatedObjects, relatedUri);
        }

        public void AddOneToManyRelationOnly<TBaseObject, TRelatedObject>(
            TBaseObject baseObject,
            string relatedObjectsKey, IEnumerable<string> relatedObjectIds,
            Uri selfUri = null, Uri relatedUri = null)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToManyRelationOnly<TBaseObject, TRelatedObject>(this, baseObject, relatedObjectsKey, relatedObjectIds, relatedUri);
        }

        public void AddOneToManyRelationOnly<TBaseObject, TRelatedObject>(
            string relatedObjectsKey, IEnumerable<string> relatedObjectIds,
            Uri selfUri = null, Uri relatedUri = null)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            Data.AddOneToManyRelationOnly<TRelatedObject>(this, relatedObjectsKey, relatedObjectIds, selfUri, relatedUri);
        }

        public void AddOneToManyRelationOnly<TBaseObject>(
            string relatedObjectsKey, Uri selfUri, Uri relatedUri)
            where TBaseObject : JsonApiResourceObject
        {
            Data.AddOneToManyRelationOnly(this, relatedObjectsKey, selfUri, relatedUri);
        }

        #endregion
    }
}