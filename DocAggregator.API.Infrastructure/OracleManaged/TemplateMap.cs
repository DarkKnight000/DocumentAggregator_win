using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Тип запроса.
        /// </summary>
        [XmlAttribute("typeID")]
        public int TypeID { get; set; }
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
        private IEnumerable<TemplateBind> _bindsContainer;

        public TemplateMap(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<TemplateMap>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            List<TemplateBind> config;

            using (StreamReader streamReader = new StreamReader(Path.GetFullPath(db.TemplateMap)))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<TemplateBind>));
                config = (List<TemplateBind>)deserializer.Deserialize(streamReader);
            }
            _bindsContainer = config;
        }

        public string GetPathByType(int typeID) => GetBindByType(typeID)?.FileName;

        public TemplateBind GetBindByType(int typeID) => _bindsContainer.Single(bind => bind.TypeID == typeID);
    }
}
