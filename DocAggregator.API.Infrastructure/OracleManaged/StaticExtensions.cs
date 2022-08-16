using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Содержит статические методы и методы расширения для работы с базой данных Oracle.
    /// </summary>
    public static class StaticExtensions
    {
        /// <summary>
        /// Выводит сообщение об ошибке и часть запроса, вызвавшую её.
        /// </summary>
        /// <remarks>
        /// В данной реализации пытается вызвать точку останова отладчика
        /// и выбрасывает исключение типа <see cref="Exception"/> с отформатированным сообщением об исходном исключении.
        /// </remarks>
        /// <param name="connection">Открытое подключение к БД.</param>
        /// <param name="exception">Отловленное исключение.</param>
        /// <param name="query">Проблемный запрос.</param>
        /// <param name="sqlResource">Ресурс запросов.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="Exception"/>
        internal static void ShowExceptionMessage(OracleConnection connection, OracleException exception, string query, SqlConnectionResource sqlResource)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            if (sqlResource == null)
            {
                throw new ArgumentNullException(nameof(sqlResource));
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
                    OracleCommand command = new OracleCommand(sqlResource.GetStringByName("P_SQLErrorIndexRetrieve"), connection);
                    command.Parameters.Add("sqltext", query);
                    command.Parameters.Add("errorpos", OracleDbType.Decimal, ParameterDirection.Output);
                    command.ExecuteNonQuery();
                    errorPosition = (int)((OracleDecimal)command.Parameters["errorpos"].Value).Value;
                    if (errorPosition < 0)
                    {
                        throw new IndexOutOfRangeException("Ошибок в запросе не обнаружено.");
                    }
                    cause = Regex.Match(query[errorPosition..], @"[\w\d\.]+").Value;
                    message += "The troubled table or view is \"" + cause + "\".\n\n" +
                        "In the following query:\n" + query;
                    break;
                default:
                    message += "In the following query:\n" + query + "\n\n";
                    message += exception.StackTrace;
                    break;
            }
            Debugger.Break();
            throw new Exception(message);
        }
    }
}
