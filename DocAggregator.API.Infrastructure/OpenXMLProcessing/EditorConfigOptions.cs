using DocAggregator.API.Core;

namespace DocAggregator.API.Infrastructure.OpenXMLProcessing
{
    /// <summary>
    /// Класс конфигурации <see cref="EditorService"/>.
    /// </summary>
    public class EditorConfigOptions : IOptions
    {
        public const string EditorConfig = "Editor";

        public string TemplatesDir { get; set; }
        public string LibreOffice { get; set; }
        public string Scripts { get; set; }
#if !DEBUG
        public string UserName { get; set; }
        public string UserPassword { get; set; }
#endif

        public string GetSection() => EditorConfig;
    }
}
