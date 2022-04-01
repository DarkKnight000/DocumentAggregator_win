using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Предстваляет модель запроса для использования в конфигурации.
    /// </summary>
    [Serializable]
    public class SqlQuery
    {
        /// <summary>
        /// Название запроса.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Тело запроса.
        /// </summary>
        [XmlText]
        public string Query { get; set; }
        /// <summary>
        /// Определяет, является ли запрос устаревшим.
        /// </summary>
        [XmlIgnore]
        public bool IsObsolete => false; // TODO: Проверить использование. Удалить, если не нужно.
    }

    /// <summary>
    /// Класс, предоставляющий ресурс именнованных запросов.
    /// </summary>
    public class SqlResource
    {
        private ILogger _logger;
        private Dictionary<string, SqlQuery> _dictionary;

        public SqlResource(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<SqlResource>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            List<SqlQuery> list;

            // deserialize the xml file
            using (StreamReader streamReader = new StreamReader(Path.GetFullPath(db.QueriesFile)))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<SqlQuery>));
                list = (List<SqlQuery>)deserializer.Deserialize(streamReader);
            }
            _dictionary = new Dictionary<string, SqlQuery>();
            foreach (var item in list)
            {
                _dictionary.Add(item.Name, item);
            }
        }

        /// <summary>
        /// Получает тело запроса по его имени.
        /// </summary>
        /// <param name="name">Имя запроса.</param>
        /// <returns>Тело запроса.</returns>
        public string GetStringByName(string name) => GetQueryByName(name).Query;

        /// <summary>
        /// Получает запрос по его имени.
        /// </summary>
        /// <param name="name">Имя запроса.</param>
        /// <returns>Объект запроса.</returns>
        public SqlQuery GetQueryByName(string name)
        {
            SqlQuery query = _dictionary[name];

            if (query == null)
                throw new ArgumentException("The query '" + name + "' is not valid.");

            if (query.IsObsolete)
            {
                _logger.Warning("Trying to get an obsolete query.");
            }
            return query;
        }
    }
}
