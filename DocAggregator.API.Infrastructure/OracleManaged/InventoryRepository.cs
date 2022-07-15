using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ILogger _logger;
        private readonly TemplateMap _templates;
        private readonly SqlConnectionResource _sqlResource;

        public InventoryRepository(SqlConnectionResource sqlRecource, TemplateMap templateMap, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<InventoryRepository>();
            _templates = templateMap;
            _sqlResource = sqlRecource;
        }

        public Inventory GetInventory(int ID)
        {
            QueryExecuterWorkspace executerWork = new()
            {
                Claim = null,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDInventInfoList_ById"), ID);
            List<ClaimField> fields = new List<ClaimField>();
            using (QueryExecuter executer = new QueryExecuter(executerWork))
            {
                while (executer.Reader.Read())
                {
                    for (int i = 0; i < executer.Reader.FieldCount; i++)
                    {
                        fields.Add(new ClaimField()
                        {
                            VerbousID = executer.Reader.GetName(i),
                            Category = string.Empty,
                            Attribute = executer.Reader.GetName(i),
                            Value = executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i),
                        });
                    }
                }
            }
            executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDInventOSList_ById"), ID);
            List<OS> oss = new List<OS>();
            using (QueryExecuter executer = new QueryExecuter(executerWork))
            {
                while (executer.Reader.Read())
                {
                    oss.Add(new OS()
                    {
                        Name = executer.Reader.GetString(0),
                        SerialNumber = executer.Reader.IsDBNull(1) ? string.Empty : executer.Reader.GetString(1),
                        InventoryNumber = executer.Reader.IsDBNull(2) ? string.Empty : executer.Reader.GetString(2),
                    });
                }
            }
            return new Inventory()
            {
                Template = _templates.GetStocktakingActTemplatePath(),
                InventoryFields = fields,
                OSs = oss,
            };
        }
    }
}
