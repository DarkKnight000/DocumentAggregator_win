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
        private MixedFieldRepository _fieldRepository;

        public ClaimRepository(SqlConnectionResource sqlResource, TemplateMap templateMap, IClaimFieldRepository fieldRepository, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimRepository>();
            _templates = templateMap;
            _sqlResource = sqlResource;
            _fieldRepository = fieldRepository as MixedFieldRepository;
        }

        public Claim GetClaim(int id)
        {
            int typeID = -1, registerSystemID = -1;
            OracleConnection connection = null;
            QueryExecuterWorkspace executerWork = new QueryExecuterWorkspace()
            {
                Claim = null,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDClaimSystemID_ByRequest"), id);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
            {
                executer.Reader.Read();
                typeID = executer.Reader.GetInt32(0);
                registerSystemID = executer.Reader.GetInt32(1);
                if (executer.Reader.Read())
                {
                    _logger.Error("When processing a claim with id {0} two or more system bindings were found. The first two: [{1}, {2}] and [{3}, {4}].",
                        id, typeID, registerSystemID, executer.Reader.GetInt32(0), executer.Reader.GetInt32(1));
                    throw new System.Exception("One claim had two or more related informational systems. See the log for more info.");
                }
                connection = executer.Connection;
            }
            _logger.Trace("Getting a template for type [{0}, {1}].", typeID, registerSystemID);
            string template = _templates.GetPathByTypeAndSystem(typeID, registerSystemID);
            if (template == null)
            {
                _logger.Error("Template has not found for claim type [{0}, {1}].", typeID, registerSystemID);
            }
            Claim result = new Claim()
            {
                ID = id,
                TypeID = typeID,
                SystemID = registerSystemID,
                Template = template,
                DbConnection = connection,
            };
            result.ClaimFields = _fieldRepository.GetFiledListByClaimId(result, false);
            result.InformationResources = _fieldRepository.GetInformationalResourcesByClaim(result);
            return result;
        }
    }
}
