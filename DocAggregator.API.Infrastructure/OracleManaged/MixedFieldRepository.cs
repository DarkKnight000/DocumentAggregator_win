using DocAggregator.API.Core;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Реализует интерфейс <see cref="IMixedFieldRepository"/> используя базу данных Oracle.
    /// </summary>
    public class MixedFieldRepository : IMixedFieldRepository
    {
        /// <summary>
        /// Подключение к БД. Тип <see cref="Lazy{T}"/> использован для поддержки инициализации поля после определения необходимых конструктору свойств.
        /// </summary>
        Lazy<OracleConnection> _lazyConnection;
        Dictionary<int, Dictionary<string, string>> _claimFieldsCache;

        /// <summary>
        /// Получает или задаёт путь к файлу запросов.
        /// </summary>
        public string QueriesSource
        {
            get => _queriesSource;
            set
            {
                _queriesSource = Path.GetFullPath(value);
            }
        }
        private string _queriesSource;

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

        /// <summary>
        /// Инициализирует объект <see cref="MixedFieldRepository"/>.
        /// </summary>
        public MixedFieldRepository()
        {
            _lazyConnection = new Lazy<OracleConnection>(delegate
            {
                return new OracleConnection(new OracleConnectionStringBuilder()
                {
                    DataSource = Server,
                    UserID = Username,
                    Password = Password,
                }.ToString());
            });
            _claimFieldsCache = new Dictionary<int, Dictionary<string, string>>();
        }

        public string GetFieldByNameOrId(int claimID, string name)
        {
            if (!_claimFieldsCache.ContainsKey(claimID))
            {
                _claimFieldsCache[claimID] = GetFields(claimID);
            }
            if (_claimFieldsCache[claimID].TryGetValue(name.ToUpper(), out string field))
            {
                return field;
            }
            return null;
        }

        /// <summary>
        /// Получает все поля из общей таблицы атрибутов и представления дополнительных атрибутов.
        /// </summary>
        /// <param name="claimID">Идентификатор заявки.</param>
        /// <returns>
        /// Полный перечень связанных с данным типом заявки атрибутами
        /// и дополными данными общего представления, основанного на данных выбранной заявки.
        /// </returns>
        private Dictionary<string, string> GetFields(int claimID)
        {
            SqlResource resource = SqlResource.GetSqlResource(QueriesSource);
            Dictionary<string, string> result = new Dictionary<string, string>();
            string attributesQuery = string.Format(resource.GetStringByName("Q_HRDAttributeIdsValues_ByRequest"), claimID);
            string viewQuery = string.Format(resource.GetStringByName("Q_HRDAddressAction_ByRequest"), claimID);
            OracleCommand command = null;
            OracleDataReader reader = null;
            try
            {
                _lazyConnection.Value.Open();
                using (command = new OracleCommand(attributesQuery, _lazyConnection.Value))
                using (reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string attributeId = reader.GetString(0);
                        string attributeVal = string.Empty;
                        if (!reader.IsDBNull(1))
                        {
                            attributeVal = reader.GetString(1);
                        }
                        result.Add(attributeId, attributeVal);
                    }
                }
                using (command = new OracleCommand(viewQuery, _lazyConnection.Value))
                using (reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string attributeName = reader.GetName(i);
                            string attributeVal = string.Empty;
                            if (!reader.IsDBNull(i))
                            {
                                attributeVal = reader.GetString(i);
                            }
                            result.Add(attributeName, attributeVal);
                        }
                    }
                }
                _lazyConnection.Value.Close();
            }
            catch (OracleException ex)
            {
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_lazyConnection.Value, ex, command.CommandText);
                }
            }
            return result;
        }
    }
}
