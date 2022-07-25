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
        public OracleConnection Connection { get; set; }
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
        readonly OracleCommand _command = null;
        readonly OracleDataReader _reader = null;

        public OracleDataReader Reader => _reader;

        public QueryExecuter(QueryExecuterWorkspace work)
        {
            try
            {
                if (work.Connection == null)
                {
                    throw new Exception("Where is the connection?");
                }
                else
                {
                    _command = new OracleCommand(work.Query, work.Connection);
                }
                _reader = _command.ExecuteReader();
            }
            catch (OracleException ex)
            {
                work.Logger.Error(ex, "An error occured when retrieving data.");
                if (_command != null)
                {
                    StaticExtensions.ShowExceptionMessage(work.Connection, ex, _command.CommandText, work.SqlReqource);
                }
                work.Connection.Close();
            }
        }

        public static OracleConnection BuildConnection(SqlConnectionResource sqlResource)
        {
            return new OracleConnection(new OracleConnectionStringBuilder()
            {
                DataSource = sqlResource.Server,
                UserID = sqlResource.Username,
                Password = sqlResource.Password,
            }.ToString());
        }

        public void Dispose()
        {
            ((IDisposable)_reader).Dispose();
            ((IDisposable)_command).Dispose();
        }
    }
}
