namespace Cike.AspNetCore.MinimalAPIs.Options
{
    public class MinimalApiRouteOptions
    {
        public string Prefix { get; set; } = "api";

        public string Version { get; set; } = "";

        public bool DisablePluralizeServiceName { get; set; } = false;

        public List<string> IgnoredUrlSuffixesInServiceNames { get; set; } = ["AppService", "Service"];

        public Dictionary<string, string> HttpMethodPrefixMapDic { get; set; } = new Dictionary<string, string>
        {
            {"Get","Get" },
            {"Find","Get" },
            {"Post","Post" },
            {"Create","Post" },
            {"Add","Post" },
            {"Upsert","Post" },
            {"Save","Post" },
            {"Set","Post" },
            {"Put","Put" },
            {"Update","Put" },
            {"Modify","Put" },
            {"Delete","Delete" },
            {"Remove","Delete" },
        };

        public string RootUrl
        {
            get
            {
                return $"{Prefix}{(Version.IsNullOrEmpty() ? "" : $"/{Version}")}";
            }
        }
    }
}
