using System;

namespace Toolroom.ApiHelper
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class HttpStatusCodeAttribute : Attribute
    {
        public int StatusCode { get; set; }

        public string DefaultMessage { get; set; }
    }
}