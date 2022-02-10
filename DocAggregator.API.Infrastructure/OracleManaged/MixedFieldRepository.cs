using DocAggregator.API.Core;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class MixedFieldRepository : IMixedFieldRepository
    {
        OracleConnection _connection;
        Dictionary<int, Dictionary<string, string>> _claimFieldsCache;

        public MixedFieldRepository()
        {
            _connection = new OracleConnection(StaticExtensions.CONNECTION_STRING);
            _claimFieldsCache = new Dictionary<int, Dictionary<string, string>>();
        }

        public string GetFieldByNameOrId(int claimID, string name)
        {
            if (!_claimFieldsCache.ContainsKey(claimID))
            {
                // Initialize fields of a claim.
                Dictionary<string, string> fieldsCache = _claimFieldsCache[claimID] = new Dictionary<string, string>();
                foreach (var f in GetValues(claimID))
                {
                    fieldsCache[f.Key.ToString()] = f.Value;
                }
                foreach (var f in GetOtherValues(claimID))
                {
                    fieldsCache[f.Key] = f.Value;
                }
            }
            if (_claimFieldsCache[claimID].TryGetValue(name.ToUpper(), out string field))
            {
                return field;
            }
            return null;
        }

        public Dictionary<int, string> GetValues(int request)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            string query = string.Format(SqlResource.GetStringByName("Q_HRDAttributeIdsValues_ByRequest"), request);
            OracleCommand command = null;
            try
            {
                _connection.Open();
                command = new OracleCommand(query, _connection);
                OracleDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int attributeId = reader.GetInt32(0);
                    string attributeVal = string.Empty;
                    if (!reader.IsDBNull(1))
                    {
                        attributeVal = reader.GetString(1);
                    }
                    result.Add(attributeId, attributeVal);
                }
                _connection.Close();
            }
            catch (OracleException ex)
            {
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText);
                }
            }
            return result;
        }

        public Dictionary<string, string> GetOtherValues(int request)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string query = string.Format(SqlResource.GetStringByName("Q_HRDAddressAction_ByRequest"), request);
            OracleCommand command = null;
            try
            {
                _connection.Open();
                command = new OracleCommand(query, _connection);
                OracleDataReader reader = command.ExecuteReader();
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
                _connection.Close();
            }
            catch (OracleException ex)
            {
                StaticExtensions.ShowExceptionMessage(_connection, ex, command.CommandText);
            }
            return result;
        }
    }
}
