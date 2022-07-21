using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
            var filePath = @"D:\Users\akkostin\source\repos\DocumentAggregator\DocAggregator.API.Infrastructure\Resources\DataBindings\Stocktaking.xml";
            XDocument desc = XDocument.Load(filePath);
            XElement invRoot = new XElement("ROOT");
            QueryExecuterWorkspace executerWork = new()
            {
                Claim = null,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            executerWork.Query = string.Format(desc.Root.Element("SqlQuery").Value, ID);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
            {
                while (executer.Reader.Read())
                {
                    for (int i = 0; i < executer.Reader.FieldCount; i++)
                    {
                        invRoot.Add(new XElement(executer.Reader.GetName(i), executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i)));
                    }
                }
            }
            executerWork.Query = string.Format(desc.Root.Element("Collection").Element("SqlQuery").Value, ID);
            XElement oses = new XElement("OSS");
            using (QueryExecuter executer = new QueryExecuter(executerWork))
            {
                int columns = executer.Reader.FieldCount;
                while (executer.Reader.Read())
                {
                    var row = new XElement(desc.Root.Element("Collection").Attribute("name").Value.ToUpper());
                    for (int i = 0; i < columns; i++)
                    {
                        row.Add(new XElement(executer.Reader.GetName(i), executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i)));
                    }
                    oses.Add(row);
                }
            }
            invRoot.Add(oses);
            return new Inventory()
            {
                Template = _templates.GetStocktakingActTemplatePath(),
                Root = invRoot,
            };
        }
    }
}
