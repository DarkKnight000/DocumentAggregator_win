using DocAggregator.API.Core;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class RepositoryConfigOptions : IOptions
    {
        public const string RepositoryConfig = "DB";

        public string QueriesFile { get; set; }
        public string TemplateMaps { get; set; }
        public string TemplateBindings { get; set; }
        public string DataSource { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }

        public string GetSection() => RepositoryConfig;
    }
}
