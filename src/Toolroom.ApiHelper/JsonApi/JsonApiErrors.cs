using System.Collections.Generic;

namespace Toolroom.ApiHelper
{
    public class JsonApiErrors : List<JsonApiErrorObject>
    {
        public void Add(string id, JsonApiLink aboutLink, int httpStatusCode, string appErrorCode, string title, string detail, object source, object meta)
        {
            Add(new JsonApiErrorObject
            {
                Id = id,
                Links = new JsonApiErrorLinkObject(aboutLink),
                Status = httpStatusCode.ToString(),
                StatusCode = httpStatusCode,
                Code = appErrorCode,
                Title = title,
                Detail = detail,
                Source = source,
                Meta = meta
            });
        }
    }
}