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
        /// Идентификатор информационной системы.
        /// </summary>
        [XmlAttribute("systemID")]
        public int SystemID { get; set; }
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

        public int Count => _bindsContainer.Count();

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

        public string GetPathByTypeAndSystem(int typeID, int systemID) => GetBindByTypeAndSystem(typeID, systemID)?.FileName;

        public TemplateBind GetBindByTypeAndSystem(int typeID, int systemID) =>
            _bindsContainer.Single(bind => bind.TypeID == typeID && bind.SystemID == systemID);
    }
}
