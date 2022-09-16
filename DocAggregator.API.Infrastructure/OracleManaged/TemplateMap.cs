using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Представляет модель связки типа заявки с его шаблоном.
    /// </summary>
    [Serializable]
    public class TemplateBind
    {
        /// <summary>
        /// Ограничивающие выбор признаки.
        /// </summary>
        [XmlAnyAttribute]
        public XmlAttribute[] Filter { get; set; }
        /// <summary>
        /// Имя файла.
        /// </summary>
        [XmlText]
        public string FileName { get; set; }
    }

    /// <summary>
    /// Класс-ресурс шаблонов заявки.
    /// </summary>
    public class TemplateMap
    {
        private ILogger _logger;
        private IDictionary<string, IEnumerable<TemplateBind>> _bindsMap;

        public IDictionary<string, IEnumerable<TemplateBind>> BindsMap => _bindsMap;

        public TemplateMap(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<TemplateMap>();
            _bindsMap = new Dictionary<string, IEnumerable<TemplateBind>>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            List<TemplateBind> config;

            foreach (var type in Directory.GetFiles(Path.GetFullPath(db.TemplateMaps), "*.xml"))
            {
                using (StreamReader streamReader = new StreamReader(type))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(List<TemplateBind>));
                    config = (List<TemplateBind>)deserializer.Deserialize(streamReader);
                }
                _bindsMap.Add(Path.GetFileNameWithoutExtension(type).ToLower(), config);
            }
        }

        public string GetTemplate(string documentType, XElement model)
        {
            if (string.IsNullOrEmpty(documentType) || !_bindsMap.ContainsKey(documentType))
            {
                throw new Exception(string.Format("Document type \"{0}\" haven't been recognized.", documentType));
            }
            var binds = _bindsMap[documentType];
            HashSet<string> affectedAttributes = new HashSet<string>();
            foreach (var bind in binds)
            {
                if (bind.Filter?.All( // If we miss a property, ignore this returning 'true'.
                        attr =>
                        {
                            affectedAttributes.Add(attr.Name.ToLower());
                            return model.Element(attr.Name.ToLower())?.Value?.Equals(attr.Value) ?? true;
                        }
                    ) ?? true)
                {
                    _logger.Trace("Have got a template for a {0} with proprties: {{{1}}}",
                        documentType,
                        string.Join(", ", bind.Filter?.Select(
                            attr => $"\"{attr.Name}\":\"{attr.Value}\""
                        ) ?? Enumerable.Empty<string>()));
                    return bind.FileName;
                }
            }
            var msg = string.Format("Template has not found for a {0} with ID = {1}. Affected attributes: {{{2}}}.",
                documentType,
                model?.Element("id")?.Value ?? "unknown",
                string.Join(",", affectedAttributes.Select(
                    attr => $"\"{attr}\":\"{model.Element(attr).Value}\""
                ) ?? Enumerable.Empty<string>()));
            var ex = new FileNotFoundException(msg);
            _logger.Error(ex, msg);
            throw ex;
        }
    }
}
