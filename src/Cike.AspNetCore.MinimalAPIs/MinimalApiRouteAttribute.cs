namespace Cike.AspNetCore.MinimalAPIs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MinimalApiRouteAttribute : Attribute
    {
        public MinimalApiRouteAttribute(string pattern, params string[] httpMethods)
        {
            Pattern = pattern;
            HttpMethods = httpMethods;
        }

        public string Pattern { get; set; }
        public IEnumerable<string> HttpMethods { get; set; } = Enumerable.Empty<string>();
    }
}
