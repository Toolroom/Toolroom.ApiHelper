using System;

namespace Toolroom.ApiHelper
{
    public class JsonClassAttribute : Attribute
    {
        public JsonClassAttribute()
        {

        }

        public JsonClassAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
