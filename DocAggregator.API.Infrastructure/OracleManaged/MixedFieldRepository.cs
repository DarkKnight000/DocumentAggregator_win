using DocAggregator.API.Core;
using Oracle.ManagedDataAccess.Client;
using System;
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
        OracleConnection _connection;
        SqlConnectionResource _sqlResource;
        Dictionary<int, Dictionary<string, string>> _claimFieldsCache;

        /// <summary>
        /// Инициализирует объект <see cref="MixedFieldRepository"/>.
        /// </summary>
        public MixedFieldRepository(SqlConnectionResource sqlResource, ILoggerFactory logger)
        {
            _logger = logger.GetLoggerFor<IClaimFieldRepository>();
            _connection = sqlResource.Connection;
            _sqlResource = sqlResource;
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

        public bool GetAccessRightByIdAndStatus(int claimID, string roleID, AccessRightStatus status)
        {
            string attributesQuery = string.Format(_sqlResource.GetStringByName("Q_HRDClaimAccessList_ByRequest"), claimID);
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
                        string roleId = reader.GetString(1);
                        if (roleId != roleID)
                        {
                            continue;
                        }
                        int? roleAction = null;
                        if (!reader.IsDBNull(2))
                        {
                            roleAction = reader.GetInt32(2);
                        }
                        if (status.HasFlag(AccessRightStatus.Allowed))
                        {
                            return roleAction.HasValue && roleAction.Value == 1;
                        }
                        else
                        {
                            return roleAction.HasValue && roleAction.Value == 0;
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claimID);
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText, _sqlResource);
                }
            }
            finally
            {
                _connection.Close();
            }
            return false;
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
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claimID);
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText, _sqlResource);
                }
            }
            finally
            {
                _connection.Close();
            }
            return result;
        }

        public IEnumerable<Tuple<string, string, string>> GetFiledListByClaimId(int claimID)
        {
            var result = new List<Tuple<string, string, string>>();
            string attributesQuery = string.Format(_sqlResource.GetStringByName("Q_HRDClaimFieldsList_ByRequest"), claimID);
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
                        string attributeName = reader.GetString(0);
                        string attributeId = reader.GetString(1);
                        string attributeVal = string.Empty;
                        if (!reader.IsDBNull(2))
                        {
                            attributeVal = reader.GetString(2);
                        }
                        result.Add(Tuple.Create(attributeName, attributeId, attributeVal));
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
                            result.Add(Tuple.Create(attributeName, attributeName, attributeVal));
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claimID);
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText, _sqlResource);
                }
            }
            finally
            {
                _connection.Close();
            }
            return result;
        }

        public IEnumerable<Tuple<string, string, string>> GetFilledAccessListByClaimId(int claimId)
        {
            var result = new List<Tuple<string, string, string>>();
            string accessListQuery = string.Format(_sqlResource.GetStringByName("Q_HRDClaimAccessList_ByRequest"), claimId);
            OracleCommand command = null;
            OracleDataReader reader = null;
            try
            {
                _connection.Open();
                using (command = new OracleCommand(accessListQuery, _connection))
                using (reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string code = reader.GetString(1);
                        string cont = string.Empty;
                        if (!reader.IsDBNull(2))
                        {
                            switch (reader.GetString(2))
                            {
                                case "0":
                                    cont = "-";
                                    break;
                                case "1":
                                    cont = "+";
                                    break;
                                default:
                                    cont = "???";
                                    break;
                            }
                        }
                        result.Add(Tuple.Create(name, code, cont));
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claimId);
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText, _sqlResource);
                }
            }
            finally
            {
                _connection.Close();
            }
            return result;
        }
    }
}
