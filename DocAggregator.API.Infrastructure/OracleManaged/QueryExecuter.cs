using DocAggregator.API.Core;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Содержит набор параметров для работы исполнительного блока.
    /// </summary>
    class QueryExecuterWorkspace
    {
        public OracleConnection Connection { get; set; }
        public ILogger Logger { get; set; }
        public SqlConnectionResource SqlReqource { get; set; }

        public QueryExecuter GetExecuterForQuery(string query)
            => new QueryExecuter(this, query);

        public QueryExecuter GetExecuterForQuery(string query, params string[] args)
            => new QueryExecuter(this, /*string.Format(*/query/*, args)*/, args);
    }

    class QueryObject
    {
        public string Value { get; set; }
        public string ColumnName { get; set; }

        public static QueryObject[] CompactObjectsWithNames(IEnumerable<object> values, IEnumerable<string> columns)
        {
            return values.Zip(
                    columns
                ).Select(
                    t => new QueryObject()
                    {
                        Value = t.First?.ToString() ?? string.Empty,
                        ColumnName = t.Second
                    }
                ).ToArray();
        }

        public override string ToString()
        {
            return Value;
        }
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

        protected OracleDataReader Reader => _reader;

        public QueryExecuter(QueryExecuterWorkspace work, string query, params string[] args)
        {
            try
            {
                if (work.Connection == null)
                {
                    throw new Exception("Where is the connection?");
                }
                _command = new OracleCommand(query, work.Connection);
                OracleDbType paramType;
                object paramVal;
                for (int i = 0; i < args.Length; i++)
                {
                    if (int.TryParse(args[i], out int number))
                    {
                        paramType = OracleDbType.Int32;
                        paramVal = number;
                    }
                    else
                    {
                        paramType = OracleDbType.Varchar2;
                        paramVal = args[i];
                    }
                    _command.Parameters.Add(new OracleParameter(i.ToString(), paramType, paramVal, System.Data.ParameterDirection.Input));
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

        public IEnumerable<string> GetHeaders() => Enumerable.Range(0, Reader.FieldCount).Select(i => Reader.GetName(i));

        public IEnumerable<ICollection<QueryObject>> GetLines()
        {
            var columnCount = Reader.FieldCount;
            var columns = GetHeaders();
            while (Reader.Read())
            {
                var results = new object[columnCount];
                Reader.GetValues(results);
                yield return QueryObject.CompactObjectsWithNames(results, columns);
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
