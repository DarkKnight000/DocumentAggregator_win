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
    /// Класс, предоставляющий ресурс именнованных запросов и единое подключение к БД.
    /// </summary>
    public class SqlConnectionResource
    {
        readonly ILogger _logger;
        readonly Dictionary<string, SqlQuery> _queriesContainer;

        /// <summary>
        /// DataSource подключения к БД.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// UserID подключения к БД.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password подключения к БД.
        /// </summary>
        public string Password { get; set; }

        public int Count => _queriesContainer.Count;

        public SqlConnectionResource(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<SqlConnectionResource>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            List<SqlQuery> list = null;

            try
            {
                // TODO: FILEWATCHER
                using (StreamReader streamReader = new StreamReader(Path.GetFullPath(db.QueriesFile)))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(List<SqlQuery>));
                    list = (List<SqlQuery>)deserializer.Deserialize(streamReader);
                }
            }
            catch (Exception ex)
            {
                RepositoryExceptionHelper.ThrowConfigurationQueriesFileFailure(ex);
            }
            _queriesContainer = new Dictionary<string, SqlQuery>();
            foreach (var item in list)
            {
                _queriesContainer.Add(item.Name, item);
            }

            Server = db.DataSource;
            Username = db.UserID;
            Password = db.Password;
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
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        public SqlQuery GetQueryByName(string name)
        {
            SqlQuery query = _queriesContainer[name];
            if (query.IsObsolete)
            {
                _logger.Warning($"Trying to get an obsolete query by name {name}.");
            }
            return query;
        }
    }
}
