using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolroom.ApiHelper
{
    public class JsonApiCollectionDocument<T> : JsonApiDocument where T : JsonBaseModel
    {
        #region Constructors

        public JsonApiCollectionDocument()
        {

        }

        public JsonApiCollectionDocument(CallResult<IEnumerable<T>> result, JsonApiErrors errors, object metadata = null, bool hasLinks = false,
            bool hasRelationships = false, bool hasIncludedData = false, Func<T, string> customIdResolver = null) : this(result.Data, errors, metadata, hasLinks, hasRelationships, hasIncludedData, customIdResolver)
        {
        }

        public JsonApiCollectionDocument(IEnumerable<T> data, JsonApiErrors errors, object metadata = null, bool hasLinks = false, bool hasRelationships = false, bool hasIncludedData = false, Func<T, string> customIdResolver = null)
        {
            if (customIdResolver == null)
            {
                if (typeof(ICustomJsonModelId).IsAssignableFrom(typeof(T)))
                {
                    customIdResolver = d => (d as ICustomJsonModelId)?.CustomJsonModelId;
                }
                else
                {
                    customIdResolver = d => d.Id.ToString();
                }
            }
            Meta = metadata;
            Errors = errors;
            if (data == null)
            {
                Data = null;
                Included = null;
            }
            else
            {
                Data = new List<JsonApiResourceObject<T>>();

                foreach (var item in data)
                {
                    JsonClassAttribute classAttrib = item.GetType().GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
                    if(classAttrib == null) { throw new Exception("No class attribute specified for " + item.GetType().Name + "."); }
                    var newDataItem = new JsonApiResourceObject<T>(customIdResolver(item), classAttrib.Name, item);
                    if (hasRelationships)
                    {
                        newDataItem.Relationships = new JsonApiRelationshipsObject();
                    }
                    Data.Add(newDataItem);
                }

                //TODO :replace is a hack to remove the "Model"-Part from the name, solve this as parameter in the future 
                Included = !hasIncludedData ? null : new JsonApiInclusionCollection();//new List<JsonApiResourceObject>();
            }

            Links = hasLinks ? new JsonApiLinksObject() : null;
        }

        public JsonApiCollectionDocument(T data, JsonApiErrors errors, object metadata = null, bool hasLinks = false, bool hasRelationships = false, bool hasIncludedData = false, bool includeFakeRealm = false)
            : this(new[] { data }, errors, metadata, hasLinks, hasRelationships, hasIncludedData)
        {

        }

        public JsonApiCollectionDocument(JsonApiErrors errors, object meta = null, JsonApiLinksObject links = null)
        {
            Meta = meta;
            Errors = errors;
            Data = null;
            Included = null;
            Links = links;
        }

        #endregion

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public List<JsonApiResourceObject<T>> Data
        {
            get
            {
                return RawData as List<JsonApiResourceObject<T>>;
            }

            set
            {
                RawData = value;
            }
        }
    }
}