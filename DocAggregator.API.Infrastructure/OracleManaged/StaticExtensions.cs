using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    internal static class StaticExtensions
    {
        internal const string CONNECTION_STRING = "Data Source=(DESCRIPTION =(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST = 10.50.12.6)(PORT = 1521)))(CONNECT_DATA =(SERVICE_NAME = WDB)));User ID=HRD_NEW_DOC;Password=123;";

        internal static void ShowExceptionMessage(OracleConnection connection, OracleException exception, string query)
        {
            if (connection == null || exception == null || query == null)
            {
                throw new ArgumentNullException("Пропущен аргумент.");
            }
            if (connection.State != ConnectionState.Open)
            {
                throw new ArgumentException("Соединение с БД закрыто или разорвано.");
            }
            string cause, message = exception.Message + "\n\n";
            int errorPosition;
            switch (exception.Number)
            {
                case 911: // ORA-00911: invalid character
                case 942: // ORA-00942: table or view does not exist
                case 12170: // ORA-12170: TNS:Connect timeout occurred
                    OracleCommand command = new OracleCommand(SqlResource.GetStringByName("P_SQLErrorIndexRetrieve"), connection);
                    command.Parameters.Add("sqltext", query);
                    command.Parameters.Add("errorpos", OracleDbType.Decimal, ParameterDirection.Output);
                    command.ExecuteNonQuery();
                    errorPosition = (int)((OracleDecimal)command.Parameters["errorpos"].Value).Value;
                    if (errorPosition < 0)
                    {
                        throw new IndexOutOfRangeException("Ошибок в запросе не обнаружено.");
                    }
                    cause = Regex.Match(query.Substring(errorPosition), @"[\w\d\.]+").Value;
                    message += "The troubled table or view is \"" + cause + "\".\n\n" +
                        "In the following query:\n" + query;
                    break;
                default:
                    message += "In the following query:\n" + query + "\n";
                    message += exception.StackTrace;
                    break;
            }
            Debugger.Break();
            throw new Exception(message);
        }
    }
}
