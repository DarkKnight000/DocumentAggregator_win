using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class DocumentModelRepository : IClaimRepository
    {
        private readonly ILogger _logger;
        private readonly TemplateMap _templates;
        private readonly SqlConnectionResource _sqlResource;

        public DocumentModelRepository(SqlConnectionResource sqlResource, TemplateMap templateMap, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<DocumentModelRepository>();
            _templates = templateMap;
            _sqlResource = sqlResource;
        }

        public Claim GetClaim(int id)
        {
            XDocument Claim = new XDocument(new XElement("Root"));
            XDocument Schema = XDocument.Load(
                @"D:\Users\akkostin\source\repos\DocumentAggregator\DocAggregator.API.Infrastructure\Resources\DataBindings\TestClaim.xml"
            );
            XElement root = Claim.Root;
            Load(Schema.Root, root);
            return new Claim()
            {
                ID = int.Parse(root.XPathSelectElement("/ID").Value),
            };
        }

        private void Load(XElement scheme, XElement document)
        {
            if (scheme.Name.Equals("SqlQuery"))
            {
                QueryExecuterWorkspace executerWork = new()
                {
                    Claim = null,
                    Logger = _logger,
                    SqlReqource = _sqlResource,
                    Query = string.Format(scheme.Value, scheme.Attribute("argument").Value),
                };
                string keyColName = scheme.Attribute("keyColumn")?.Value, valColName = scheme.Attribute("valColumn")?.Value;
                if (keyColName != null && valColName != null)
                {
                    using (QueryExecuter executer = new QueryExecuter(executerWork))
                    {
                        executer.Reader.Read();
                        int keyCol = -1, valCol = -1, fieldCount = executer.Reader.FieldCount;
                        for (int i = 0; i < fieldCount; i++)
                        {
                            if (executer.Reader.GetName(i).Equals(keyColName, StringComparison.OrdinalIgnoreCase))
                            {
                                keyCol = i;
                            }
                            if (executer.Reader.GetName(i).Equals(valColName, StringComparison.OrdinalIgnoreCase))
                            {
                                valCol = i;
                            }
                        }
                        if ((keyCol | valCol) < 0) // If one of both is negative.
                        {
                            throw new Exception("Received answer had no expected columns.");
                        }
                        XElement list = new XElement(scheme.Parent.Attribute("name").Value);
                        document.Add(list);
                        do
                        {
                            XElement field = new XElement(executer.Reader.GetString(keyCol), executer.Reader.GetString(valCol));
                        }
                        while (executer.Reader.Read());
                    }
                }
                else
                {
                    using (QueryExecuter executer = new QueryExecuter(executerWork))
                    {
                        executer.Reader.Read();
                    }
                }
            }
            foreach (XElement elem in scheme.Elements("List"))
            {
                ;
            }
        }
    }
}
