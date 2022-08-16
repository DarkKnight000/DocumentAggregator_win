using DocAggregator.API.Core;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class ModelBind
    {
        private ILogger _logger;
        private StringDictionary _dataBindings;

        public StringDictionary DataBindings => _dataBindings;

        public ModelBind(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ModelBind>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            _dataBindings = new StringDictionary();
            foreach (var filePath in Directory.GetFiles(db.TemplateBindings, "*.xml"))
            {
                _dataBindings.Add(Path.GetFileNameWithoutExtension(filePath).ToLower(), filePath);
            }
        }

        public XDocument GetBind(string documentType)
        {
            return XDocument.Load(_dataBindings[documentType]);
        }
    }
}
