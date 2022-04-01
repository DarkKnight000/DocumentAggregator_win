using DocAggregator.API.Core;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.IO;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Реализует интерфейс <see cref="IClaimFieldRepository"/> используя базу данных Oracle.
    /// </summary>
    public class MixedFieldRepository : IClaimFieldRepository
    {
        private ILogger _logger;

        /// <summary>
        /// Подключение к БД.
        /// </summary>
        OracleConnection _connection;
        SqlResource _sqlResource;
        Dictionary<int, Dictionary<string, string>> _claimFieldsCache;

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
        public MixedFieldRepository(SqlResource sqlResource, IOptionsFactory optionsFactory, ILoggerFactory logger)
        {
            _logger = logger.GetLoggerFor<IClaimFieldRepository>();
            _sqlResource = sqlResource;
            _claimFieldsCache = new Dictionary<int, Dictionary<string, string>>();

            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            Server = db.DataSource;
            Username = db.UserID;
            Password = db.Password;

            _connection = new OracleConnection(new OracleConnectionStringBuilder()
            {
                DataSource = Server,
                UserID = Username,
                Password = Password,
            }.ToString());
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
            Dictionary<string, string> result = new Dictionary<string, string>();
            string attributesQuery = string.Format(_sqlResource.GetStringByName("Q_HRDAttributeIdsValues_ByRequest"), claimID);
            string viewQuery = string.Format(_sqlResource.GetStringByName("Q_HRDAddressAction_ByRequest"), claimID);
            OracleCommand command = null;
            OracleDataReader reader = null;
            try
            {
                _connection.Open();
                using (command = new OracleCommand(attributesQuery, _connection))
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
                using (command = new OracleCommand(viewQuery, _connection))
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
                _connection.Close();
            }
            catch (OracleException ex)
            {
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText, _sqlResource);
                }
            }
            return result;
        }
    }
}
