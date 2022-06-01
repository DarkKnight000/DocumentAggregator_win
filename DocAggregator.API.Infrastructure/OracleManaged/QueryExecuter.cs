using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Oracle.ManagedDataAccess.Client;
using System;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Содержит набор параметров для работы исполнительного блока.
    /// </summary>
    struct QueryExecuterWorkspace
    {
        public string Query { get; set; }
        public Claim Claim { get; set; }
        public ILogger Logger { get; set; }
        public SqlConnectionResource SqlReqource { get; set; }
    }

    /// <summary>
    /// Объект исполнителя запроса.
    /// </summary>
    /// <remarks>
    /// Рекомендуется использовать в блоке using.
    /// </remarks>
    class QueryExecuter : IDisposable
    {
        OracleConnection _connection = null;
        OracleCommand _command = null;
        OracleDataReader _reader = null;

        public OracleConnection Connection => _connection;
        public OracleDataReader Reader => _reader;

        public QueryExecuter(QueryExecuterWorkspace work)
        {
            try
            {
                if (work.Claim == null)
                {
                    _connection = new OracleConnection(new OracleConnectionStringBuilder()
                    {
                        DataSource = work.SqlReqource.Server,
                        UserID = work.SqlReqource.Username,
                        Password = work.SqlReqource.Password,
                    }.ToString());
                    _connection.Open();
                    _command = new OracleCommand(work.Query, _connection);
                }
                else
                {
                    _command = new OracleCommand(work.Query, work.Claim.DbConnection as OracleConnection);
                }
                _reader = _command.ExecuteReader();
            }
            catch (OracleException ex)
            {
                work.Logger.Error(ex, "An error occured when retrieving data. ClaimID: {0}.", work.Claim.ID);
                if (_command != null)
                {
                    StaticExtensions.ShowExceptionMessage(work.Claim.DbConnection as OracleConnection, ex, _command.CommandText, work.SqlReqource);
                }
                work.Claim.DbConnection.Close();
            }
        }

        public void Dispose()
        {
            ((IDisposable)_reader).Dispose();
            ((IDisposable)_command).Dispose();
        }
    }
}
