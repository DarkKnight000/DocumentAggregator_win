using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
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

        public ClaimField GetFieldByNameOrId(Claim claim, string name)
        {
            if (!_claimFieldsCache.ContainsKey(claim.ID))
            {
                _claimFieldsCache[claim.ID] = GetFields(claim);
            }
            if (_claimFieldsCache[claim.ID].TryGetValue(name.ToUpper(), out string field))
            {
                return new ClaimField() { Value = field };
            }
            return null;
        }

        public AccessRightField GetAccessRightByIdAndStatus(Claim claim, string roleID, AccessRightStatus status)
        {
            string attributesQuery = string.Format(_sqlResource.GetStringByName("Q_HRDClaimAccessList_ByRequest"), claim);
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
                            return new AccessRightField()
                            {
                                Status = roleAction.HasValue && roleAction.Value == 1 ? AccessRightStatus.Allowed : AccessRightStatus.Denied
                            };
                        }
                        else
                        {
                            return new AccessRightField()
                            {
                                Status = roleAction.HasValue && roleAction.Value == 0 ? AccessRightStatus.Allowed : AccessRightStatus.Denied
                            };
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claim);
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText, _sqlResource);
                }
            }
            finally
            {
                _connection.Close();
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
        private Dictionary<string, string> GetFields(Claim claim)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string attributesQuery = string.Format(_sqlResource.GetStringByName("Q_HRDAttributeIdsValues_ByRequest"), claim.ID);
            string viewQuery = string.Format(_sqlResource.GetStringByName("Q_HRDAddressAction_ByRequest"), claim.ID);
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
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claim.ID);
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

        public IEnumerable<ClaimField> GetFiledListByClaimId(Claim claim)
        {
            var result = new List<ClaimField>();
            string attributesQuery = string.Format(_sqlResource.GetStringByName("Q_HRDClaimFieldsList_ByRequest"), claim.ID);
            string viewQuery = string.Format(_sqlResource.GetStringByName("Q_HRDAddressAction_ByRequest"), claim.ID);
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
                        string categoryName = reader.GetString(0);
                        string attributeName = reader.GetString(1);
                        string attributeId = reader.GetString(2);
                        string attributeVal = string.Empty;
                        if (!reader.IsDBNull(3))
                        {
                            attributeVal = reader.GetString(3);
                        }
                        result.Add(new ClaimField() {
                            VerbousID = attributeId,
                            Category = categoryName,
                            Attribute = attributeName,
                            Value = attributeVal,
                        });
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
                            result.Add(new ClaimField() {
                                VerbousID = attributeName,
                                Category = attributeName,
                                Attribute = string.Empty,
                                Value = attributeVal,
                            });
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claim.ID);
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

        public IEnumerable<AccessRightField> GetFilledAccessListByClaimId(Claim claim)
        {
            var result = new List<AccessRightField>();
            string accessListQuery = string.Format(_sqlResource.GetStringByName("Q_HRDClaimAccessList_ByRequest"), claim.ID);
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
                        AccessRightStatus stat = AccessRightStatus.NotMentioned;
                        if (!reader.IsDBNull(2))
                        {
                            switch (reader.GetString(2))
                            {
                                case "0":
                                    stat = AccessRightStatus.Denied;
                                    break;
                                case "1":
                                    stat = AccessRightStatus.Allowed;
                                    break;
                                default:
                                    break;
                            }
                        }
                        result.Add(new AccessRightField() {
                            NumeralID = int.Parse(code),
                            Name = name,
                            Status = stat,
                        });
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim filds. ClaimID: {0}.", claim.ID);
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
