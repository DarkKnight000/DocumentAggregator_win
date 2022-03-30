using DocAggregator.API.Core;

namespace DocAggregator.API.Infrastructure.OpenXMLProcessing
{
    public class EditorConfigOptions : IOptions
    {
        public const string EditorConfig = "Editor";

        public string TemplatesDir { get; set; }
        public string OutputDir { get; set; }
        public string LibreOffice { get; set; }
        public string Scripts { get; set; }

        public string GetSection() => EditorConfig;
    }
}
