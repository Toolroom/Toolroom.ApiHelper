namespace Toolroom.ApiHelper
{
    public class JsonApiErrorLinkObject
    {
        public JsonApiErrorLinkObject(JsonApiLink about)
        {
            About = about;
        }

        public JsonApiLink About { get; set; }
    }
}