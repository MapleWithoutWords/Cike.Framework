namespace Cike.AspNetCore.MinimalAPIs
{
    public class MinimalApiRouteOptions
    {
        public string Prefix { get; set; } = "api";

        public string Version { get; set; } = "";

        public bool DisablePluralizeServiceName { get; set; } = false;

        public string[] IgnoredUrlSuffixesInServiceNames { get; set; } = ["Service", "AppService"];

        public List<string> GetPrefixes { get; set; } = ["Get","Find"];

        public List<string> PostPrefixes { get; set; } = ["Post", "Create", "Add", "Upsert", "Save", "Set"];

        public List<string> PutPrefixes { get; set; } = ["Put", "Update", "Modify"];

        public List<string> DeletePrefixes { get; set; } = ["Delete", "Remove"];
    }
}
