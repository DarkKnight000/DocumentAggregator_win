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
        private static SqlResource _singletone;
        private Dictionary<string, SqlQuery> _dictionary;
        private string _configuration;

        SqlResource(string file, ILogger logger)
        {
            _logger = logger;
            _configuration = file;
            List<SqlQuery> list;

            // deserialize the xml file
            using (StreamReader streamReader = new StreamReader(file))
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
        /// Получает статически хранимый синглтон ресурса запросов.
        /// </summary>
        /// <param name="configuration">Путь к файлу с запросами. Разрешён null, если прежде вызван с корректным файлом.</param>
        /// <returns>Ресурс запросов.</returns>
        public static SqlResource GetSqlResource(string configuration, ILogger logger)
        {
            if (_singletone == null)
            {
                if (configuration == null)
                {
                    throw new ArgumentNullException(nameof(configuration));
                }
                _singletone = new SqlResource(configuration, logger);
            }
            else if (configuration != null && _singletone._configuration != configuration)
            {
                throw new ArgumentException("Попытка получить ресурс другой конфигурации.", nameof(configuration));
            }
            return _singletone;
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
