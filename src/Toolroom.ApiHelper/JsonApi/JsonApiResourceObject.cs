using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Toolroom.ApiHelper
{
    public abstract class JsonApiResourceObject : JsonApiResource
    {
        protected JsonApiResourceObject(string id, string type, object attributes)
        {
            Id = id;
            Type = type;
            var jsonBaseModel = attributes as JsonBaseModel;
            if (jsonBaseModel != null)
            {
                int elementId;
                if (int.TryParse(Id, out elementId))
                    jsonBaseModel.Id = elementId;
            }
            Attributes = attributes;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiRelationshipsObject Relationships { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }

        public object Attributes { get; set; }

        public T ToModel<T>() where T : JsonBaseModel
        {
            string jsonString = Attributes as string;
            T result = null;
            try {
                result = Newtonsoft.Json.Linq.JObject.Parse(jsonString)?.ToObject<T>();
            }
            catch (Exception e)
            {
            }
            int id = -1;
            int.TryParse(Id, out id);
            result.Id = id;
            return result;
        }
    }

    public class JsonApiResourceObject<TMain> : JsonApiResourceObject
    {
        public JsonApiResourceObject(string id, string type, TMain attributes) : base(id, type, attributes)
        {

        }

        public new TMain Attributes
        {
            get
            {
                return (TMain)base.Attributes;
            }

            set
            {
                base.Attributes = value;
            }
        }

        #region 1:1

        public void AddOneToOneRelation<TRelatedObject>(
            JsonApiDocument document,
            TRelatedObject relatedObject)
            where TRelatedObject : JsonApiResourceObject
        {
            AddOneToOneRelation(document, relatedObject.Attributes.GetType().Name, relatedObject);
        }

        public void AddOneToOneRelation<TRelatedObject>(
            JsonApiDocument document,
            string relatedObjectKey, TRelatedObject relatedObject)
            where TRelatedObject : JsonApiResourceObject
        {
            if (relatedObject == null)
            {
                return;
            }

            string relatedTypeName = "unknown";

            var relatedGenericTypeArgument = relatedObject.GetType().GetGenericArguments().FirstOrDefault();
            if (relatedGenericTypeArgument != null)
            {
                var relatedJsonClassAttribute = relatedGenericTypeArgument.GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
                relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;
            }

            if (Relationships == null)
            {
                Relationships = new JsonApiRelationshipsObject();
            }

            Relationships.Add(relatedObjectKey, new JsonApiToOneRelationship
            {
                Data = new JsonApiResourceIdentiferObject(relatedObject.Id, relatedTypeName)
            });

            if (document?.Included != null)
            {
                document.Included.Add(relatedObject);
            }
        }

        public void AddOneToOneRelation<TBaseObject, TRelatedObject>(
            JsonApiDocument document,
            TBaseObject baseObject,
            TRelatedObject relatedObject)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            AddOneToOneRelation(document, baseObject, relatedObject.Attributes.GetType().Name, relatedObject);
        }

        public void AddOneToOneRelation<TBaseObject, TRelatedObject>(
            JsonApiDocument document,
            TBaseObject baseObject,
            string relatedObjectKey, TRelatedObject relatedObject)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            if (relatedObject == null)
            {
                return;
            }

            string relatedTypeName = "unknown";

            var relatedGenericTypeArgument = relatedObject.GetType().GetGenericArguments().FirstOrDefault();
            if (relatedGenericTypeArgument != null)
            {
                var relatedJsonClassAttribute = relatedGenericTypeArgument.GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
                relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;
            }

            if (baseObject.Relationships == null)
            {
                baseObject.Relationships = new JsonApiRelationshipsObject();
            }

            baseObject.Relationships.Add(relatedObjectKey, new JsonApiToOneRelationship
            {
                Data = new JsonApiResourceIdentiferObject(relatedObject.Id, relatedTypeName)
            });

            if (document?.Included != null)
            {
                document.Included.Add(relatedObject);
            }
        }

        public void AddOneToOneRelationOnly<TRelatedObject>(
            JsonApiDocument document,
            string relatedObjectKey, string relatedObjectId, Uri selfUri = null, Uri relatedUri = null)
            where TRelatedObject : JsonApiResourceObject
        {
            if (relatedObjectId == null && relatedUri == null)
            {
                return;
            }

            var relatedJsonClassAttribute = typeof(TRelatedObject).GetGenericArguments().FirstOrDefault()?.GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
            var relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;

            if (Relationships == null)
            {
                Relationships = new JsonApiRelationshipsObject();
            }

            Relationships.Add(relatedObjectKey, new JsonApiToOneRelationship
            {
                Data = string.IsNullOrWhiteSpace(relatedObjectId) ? null : new JsonApiResourceIdentiferObject(relatedObjectId, relatedTypeName),
                Links = selfUri == null && relatedUri == null ? null : new JsonApiLinksObject(selfUri == null ? null : new JsonApiLink(selfUri.AbsoluteUri), new JsonApiLink(relatedUri == null ? null : relatedUri.AbsoluteUri))
            });
        }

        public void AddOneToOneRelationOnly<TBaseObject, TRelatedObject>(
            JsonApiDocument document,
            TBaseObject baseObject,
            string relatedObjectKey, string relatedObjectId, Uri selfUri = null, Uri relatedUri = null)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            if (relatedObjectId == null && relatedUri == null)
            {
                return;
            }

            var relatedJsonClassAttribute = typeof(TRelatedObject).GetGenericArguments().FirstOrDefault()?.GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
            var relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;

            if (baseObject.Relationships == null)
            {
                baseObject.Relationships = new JsonApiRelationshipsObject();
            }

            baseObject.Relationships.Add(relatedObjectKey, new JsonApiToOneRelationship
            {
                Data = string.IsNullOrWhiteSpace(relatedObjectId) ? null : new JsonApiResourceIdentiferObject(relatedObjectId, relatedTypeName),
                Links = selfUri == null && relatedUri == null ? null : new JsonApiLinksObject(selfUri == null ? null : new JsonApiLink(selfUri.AbsoluteUri), new JsonApiLink(relatedUri == null ? null : relatedUri.AbsoluteUri))
            });

        }

        #endregion

        #region 1:n

        public void AddOneToManyRelation<TBaseObject, TRelatedObject>(
            JsonApiDocument document,
            TBaseObject baseObject,
            string relatedObjectsKey, IEnumerable<TRelatedObject> relatedObjects,
            Uri selfUri = null, Uri relatedUri = null)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            //var relatedJsonClassAttribute = typeof(TRelatedObject).GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
            //var relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;

            string relatedTypeName = "unknown";

            var relatedGenericTypeArgument = relatedObjects.GetType().GetGenericArguments().FirstOrDefault();
            if (relatedGenericTypeArgument != null)
            {
                var relatedJsonClassAttribute = relatedGenericTypeArgument.GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
                relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;
            }

            if (Relationships == null)
            {
                Relationships = new JsonApiRelationshipsObject();
            }
            Relationships.Add(relatedObjectsKey,
                new JsonApiToManyRelationship()
                {
                    Data = relatedObjects.Select(_ => new JsonApiResourceIdentiferObject(_.Id, relatedTypeName)),
                    Links = selfUri == null && relatedUri == null ? null : new JsonApiLinksObject(selfUri == null ? null : new JsonApiLink(selfUri.AbsoluteUri), new JsonApiLink(relatedUri == null ? null : relatedUri.AbsoluteUri))
                });

            if (document?.Included != null)
            {
                foreach (var r in relatedObjects)
                {
                    document.Included.Add(r);
                }
            }
        }


        public void AddOneToManyRelationOnly<TBaseObject, TRelatedObject>(
            JsonApiDocument document,
            TBaseObject baseObject,
            string relatedObjectsKey, IEnumerable<string> relatedObjectIds,
            Uri selfUri = null, Uri relatedUri = null)
            where TBaseObject : JsonApiResourceObject
            where TRelatedObject : JsonApiResourceObject
        {
            var relatedJsonClassAttribute = typeof(TRelatedObject).GetGenericArguments().FirstOrDefault()?.GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
            var relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;

            if (baseObject.Relationships == null)
            {
                baseObject.Relationships = new JsonApiRelationshipsObject();
            }

            baseObject.Relationships.Add(relatedObjectsKey, new JsonApiToManyRelationship()
            {
                Data = relatedObjectIds == null ? null : relatedObjectIds.Select(_ => new JsonApiResourceIdentiferObject(_, relatedTypeName)),
                Links = selfUri == null && relatedUri == null ? null : new JsonApiLinksObject(selfUri == null ? null : new JsonApiLink(selfUri.AbsoluteUri), new JsonApiLink(relatedUri == null ? null : relatedUri.AbsoluteUri))
            });
        }

        public void AddOneToManyRelationOnly<TRelatedObject>(
            JsonApiDocument document,
            string relatedObjectsKey, IEnumerable<string> relatedObjectIds,
            Uri selfUri = null, Uri relatedUri = null) where TRelatedObject : JsonApiResourceObject
        {
            var relatedJsonClassAttribute = typeof(TRelatedObject).GetGenericArguments().FirstOrDefault()?.GetCustomAttributes(typeof(JsonClassAttribute), false).FirstOrDefault() as JsonClassAttribute;
            var relatedTypeName = relatedJsonClassAttribute == null ? nameof(TRelatedObject) : relatedJsonClassAttribute.Name;

            if (Relationships == null)
            {
                Relationships = new JsonApiRelationshipsObject();
            }

            Relationships.Add(relatedObjectsKey, new JsonApiToManyRelationship
            {
                Data = relatedObjectIds == null ? null : relatedObjectIds.Select(_ => new JsonApiResourceIdentiferObject(_, relatedTypeName)),
                Links = selfUri == null && relatedUri == null ? null : new JsonApiLinksObject(selfUri == null ? null : new JsonApiLink(selfUri.AbsoluteUri), new JsonApiLink(relatedUri == null ? null : relatedUri.AbsoluteUri))
            });
        }

        public void AddOneToManyRelationOnly(
            JsonApiDocument document,
            string relatedObjectsKey, Uri selfUri, Uri relatedUri)
        {
            if (Relationships == null)
            {
                Relationships = new JsonApiRelationshipsObject();
            }

            Relationships.Add(relatedObjectsKey, new JsonApiToManyRelationship
            {
                Links = selfUri == null && relatedUri == null ? null : new JsonApiLinksObject(selfUri == null ? null : new JsonApiLink(selfUri.AbsoluteUri), new JsonApiLink(relatedUri == null ? null : relatedUri.AbsoluteUri))
            });
        }

        #endregion
    }
}