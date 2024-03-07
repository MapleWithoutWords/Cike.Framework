namespace Cike.AspNetCore.MinimalAPIs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MinimalApiRouteAttribute : Attribute
    {
        public string Pattern { get; set; }
        public IEnumerable<string> HttpMethods { get; set; } = Enumerable.Empty<string>();

        public MinimalApiRouteAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}
