using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Oracle.ManagedDataAccess.Client;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Реализует интерфейс <see cref="IClaimRepository"/> на основе базы данных Oracle.
    /// </summary>
    public class ClaimRepository : IClaimRepository
    {
        private ILogger _logger;
        private TemplateMap _templates;
        private SqlConnectionResource _sqlResource;

        public ClaimRepository(SqlConnectionResource sqlResource, TemplateMap templateMap, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimRepository>();
            _templates = templateMap;
            _sqlResource = sqlResource;
        }

        public Claim GetClaim(int id)
        {
            int typeID = -1, systemID = -1;
            string claimInfoQuery = string.Format(_sqlResource.GetStringByName("Q_HRDClaimSystemType_ByRequest"), id);
            OracleConnection connection = new OracleConnection(new OracleConnectionStringBuilder()
            {
                DataSource = _sqlResource.Server,
                UserID = _sqlResource.Username,
                Password = _sqlResource.Password,
            }.ToString());
            OracleCommand command = null;
            OracleDataReader reader = null;
            try
            {
                connection.Open();
                using (command = new OracleCommand(claimInfoQuery, connection))
                using (reader = command.ExecuteReader())
                {
                    reader.Read();
                    typeID = reader.GetInt32(0);
                    systemID = reader.GetInt32(1);
                    if (reader.Read())
                    {
                        _logger.Error("When processing a claim with id {0} two or more system bindings were found. The first two: [{1}, {2}] and [{3}, {4}].",
                            id, typeID, systemID, reader.GetInt32(0), reader.GetInt32(1));
                        throw new System.Exception("One claim had two or more related informational systems. See the log for more info.");
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured when retrieving claim information. ClaimID: {0}", id);
                if (command != null)
                {
                    StaticExtensions.ShowExceptionMessage(connection, ex, command.CommandText, _sqlResource);
                }
                connection.Close();
            }
            _logger.Trace("Getting a template for type [{0}, {1}].", typeID, systemID);
            string template = _templates.GetPathByTypeAndSystem(typeID, systemID);
            if (template == null)
            {
                _logger.Error("Template has not found for claim type [{0}, {1}].", typeID, systemID);
            }
            Claim result = new Claim()
            {
                ID = id,
                TypeID = typeID,
                SystemID = systemID,
                Template = template,
                DbConnection = connection,
            };
            return result;
        }
    }
}
