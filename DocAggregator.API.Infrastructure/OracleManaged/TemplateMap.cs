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
            List<TemplateBind> config = null;
            string[] files = null;
            try
            {
                files = Directory.GetFiles(Path.GetFullPath(db.TemplateMaps), "*.xml");
            }
            catch (Exception ex)
            {
                RepositoryExceptionHelper.ThrowConfigurationTemplateFolderFailure(ex);
            }
            foreach (var type in files)
            {
                try
                {
                    using (StreamReader streamReader = new StreamReader(type))
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(List<TemplateBind>));
                        config = (List<TemplateBind>)deserializer.Deserialize(streamReader);
                    }
                }
                catch (Exception ex)
                {
                    RepositoryExceptionHelper.ThrowConfigurationTemplateFileFailure(type, ex);
                }
                _bindsMap.Add(Path.GetFileNameWithoutExtension(type).ToLower(), config);
            }
        }

        /// <summary>
        /// Получает путь к шаблону по типу документа и данных его модели.
        /// </summary>
        /// <param name="documentType">Тип документа.</param>
        /// <param name="model">Модель документа.</param>
        /// <returns>Путь к документу шаблона.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="SolvableException"/>
        public string GetTemplate(string documentType, XElement model)
        {
            if (string.IsNullOrEmpty(documentType) || !_bindsMap.ContainsKey(documentType))
            {
                RepositoryExceptionHelper.ThrowTemplateNotFoundFailure(documentType);
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
            RepositoryExceptionHelper.ThrowTemplateNotMatchedFailure(documentType,
                model?.Element("id")?.Value ?? "unknown",
                string.Join(",", affectedAttributes.Select(
                    attr => $"\"{attr}\":\"{model.Element(attr).Value}\""
                ) ?? Enumerable.Empty<string>()));
            return null;
        }
    }
}
