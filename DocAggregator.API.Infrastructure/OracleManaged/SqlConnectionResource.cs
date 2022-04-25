﻿using DocAggregator.API.Core;
using Oracle.ManagedDataAccess.Client;
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
        private ILogger _logger;
        private Dictionary<string, SqlQuery> _queriesContainer;

        /// <summary>
        /// Подключение к БД.
        /// </summary>
        public OracleConnection Connection { get; set; }

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

        public SqlConnectionResource(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<SqlConnectionResource>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            List<SqlQuery> list;

            // deserialize the xml file
            using (StreamReader streamReader = new StreamReader(Path.GetFullPath(db.QueriesFile)))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<SqlQuery>));
                list = (List<SqlQuery>)deserializer.Deserialize(streamReader);
            }
            _queriesContainer = new Dictionary<string, SqlQuery>();
            foreach (var item in list)
            {
                _queriesContainer.Add(item.Name, item);
            }

            Server = db.DataSource;
            Username = db.UserID;
            Password = db.Password;

            Connection = new OracleConnection(new OracleConnectionStringBuilder()
            {
                DataSource = Server,
                UserID = Username,
                Password = Password,
            }.ToString());
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
            if (!_queriesContainer.ContainsKey(name))
            {
                throw new ArgumentException("The query '" + name + "' is not valid.");
            }
            SqlQuery query = _queriesContainer[name];
            if (query.IsObsolete)
            {
                _logger.Warning("Trying to get an obsolete query.");
            }
            return query;
        }
    }
}