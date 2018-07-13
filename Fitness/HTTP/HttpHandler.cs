using System;

namespace Fitness
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpHandler : Attribute
    {
        public string Url
        {
            get;
            private set;
        }

        public HttpHandler(string url)
        {
            Url = url;
        }
    }
}