using DocAggregator.API.Core;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Реализует интерфейс <see cref="IClaimRepository"/> на основе базы данных Oracle.
    /// </summary>
    public class ClaimRepository : IClaimRepository
    {
        private ILogger _logger;
        private TemplateMap _templates;

        public ClaimRepository(TemplateMap templateMap, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimRepository>();
            _templates = templateMap;
        }

        public Claim GetClaim(int id)
        {
            // TODO: Get id from the db
            /*
             * select req.request_type_id
             * from hrd.request_hdr req
             * where req.request_hrd_id = {0}
             */
            int typeID = 10;
            _logger.Trace("Getting a template for type [{0}].", typeID);
            string template = _templates.GetPathByType(typeID);
            if (template == null)
            {
                _logger.Error("Template has not found for claim type [{0}].", typeID);
            }
            Claim result = new Claim()
            {
                ID = id,
                Template = template,
            };
            return result;
        }
    }
}
