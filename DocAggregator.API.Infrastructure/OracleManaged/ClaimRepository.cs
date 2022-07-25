using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Реализует интерфейс <see cref="IClaimRepository"/> на основе базы данных Oracle.
    /// </summary>
    public class ClaimRepository : IClaimRepository
    {
        private readonly ILogger _logger;
        private readonly TemplateMap _templates;
        private readonly SqlConnectionResource _sqlResource;
        private readonly MixedFieldRepository _fieldRepository;

        public ClaimRepository(SqlConnectionResource sqlResource, TemplateMap templateMap, IClaimFieldRepository fieldRepository, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimRepository>();
            _templates = templateMap;
            _sqlResource = sqlResource;
            _fieldRepository = fieldRepository as MixedFieldRepository;
        }

        public Claim GetClaim(int id)
        {
            var filePath = @"D:\Users\akkostin\source\repos\DocumentAggregator\DocAggregator.API.Infrastructure\Resources\DataBindings\TestClaim.xml";
            XDocument desc = XDocument.Load(filePath);
            //XElement altRoot = ComputeRoot(desc.Root);
            int typeID = -1, registerSystemID = -1;
            XElement partRoot = new XElement("ROOT", new XElement("ID", id));
            OracleConnection connection = null;
            QueryExecuterWorkspace executerWork = new()
            {
                Claim = null,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDClaimSystemID_ByRequest"), id);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
            {
                executer.Reader.Read();
                //Setting Initials
                typeID = executer.Reader.GetInt32(0);
                partRoot.Add(new XElement("TYPEID", typeID));
                registerSystemID = executer.Reader.GetInt32(1);
                partRoot.Add(new XElement("SYSTEMID", registerSystemID));
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
                var msg = string.Format("Template has not found for claim type [{0}, {1}].", typeID, registerSystemID);
                var ex = new System.IO.FileNotFoundException(msg);
                _logger.Error(ex, msg);
                throw ex;
            }
            partRoot.Add(new XElement("TEMPLATE", template));
            Claim result = new()
            {
                ID = id,
                TypeID = typeID,
                Root = partRoot,
                SystemID = registerSystemID,
                Template = template,
                DbConnection = connection,
            };
            executerWork.Claim = result;
            executerWork.Query = string.Format(desc.Root.Element("Collection").Element("Query").Value, result.ID);
            XElement partFields = new XElement("ATTRIBUTES");
            partRoot.Add(partFields);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    var nodeName = executer.Reader.IsDBNull(0) ? "-" : executer.Reader.GetString(0);
                    partFields.Add(new XElement("ITEM", new XAttribute("index", nodeName), executer.Reader.IsDBNull(1) ? string.Empty : executer.Reader.GetString(1)));
                }
            executerWork.Query = string.Format(desc.Root.Elements("Collection").Where((n) => n.Attribute("name").Value.Equals("Custom")).Single().Element("Query").Value, result.ID);
            XElement partCustomFields = new XElement("CUSTOM");
            partRoot.Add(partCustomFields);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    for (int i = 0; i < executer.Reader.FieldCount; i++)
                    {
                        partCustomFields.Add(new XElement(executer.Reader.GetName(i), executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i)));
                    }
                }
            result.ClaimFields = _fieldRepository.GetFiledListByClaimId(result, false);
            var blockFor = desc.Root.Elements("Table").Where((n) => n.Attribute("name").Value.Equals("Resources")).Single().Element("For");
            executerWork.Query = string.Format(blockFor.Value, result.ID);
            XElement partResources = new XElement("RESOURCES");
            partRoot.Add(partResources);
            List<string> listOfLines = new List<string>();
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    int columns = executer.Reader.FieldCount;
                    int indexColumn = -1;
                    for (int i = 0; i < columns; i++)
                    {
                        if (executer.Reader.GetName(i).Equals(blockFor.Attribute("itemIndexColumn").Value.ToUpper()))
                        {
                            indexColumn = i;
                        }
                    }
                    listOfLines.Add(executer.Reader.GetString(0));
                    var partItem = new XElement("ITEM");
                    for (int i = 0; i < columns; i++)
                    {
                        if (i == indexColumn)
                        {
                            partItem.Add(new XAttribute("index", executer.Reader.GetString(indexColumn)));
                            continue;
                        }
                        partItem.Add(new XElement(executer.Reader.GetName(i).ToUpper(), executer.Reader.GetString(i)));
                    }
                    partResources.Add(partItem);
                    partResources.Elements().Last().Add(new XElement("RIGHTS"));
                }
            var blockGet = desc.Root.Elements("Table").Where((n) => n.Attribute("name").Value.Equals("Resources")).Single().Element("Get");
            var blockTableQuery = blockGet.Element("Query");
            foreach (var lineArg in listOfLines)
            {
                executerWork.Query = string.Format(blockTableQuery.Value, lineArg, result.ID);
                using (QueryExecuter executer = new QueryExecuter(executerWork))
                    while (executer.Reader.Read())
                    {
                        XElement partAccessField = new XElement("ITEM");
                        string infoResourceId = null;
                        int columns = executer.Reader.FieldCount;
                        int indexColumn = -1, groupColumn = -1;
                        for (int i = 0; i < columns; i++)
                        {
                            if (executer.Reader.GetName(i).Equals(blockTableQuery.Attribute("itemIndexColumn").Value.ToUpper()))
                            {
                                indexColumn = i;
                            }
                            if (executer.Reader.GetName(i).Equals(blockTableQuery.Attribute("groupColumn").Value.ToUpper()))
                            {
                                groupColumn = i;
                            }
                        }
                        for (int i = 0; i < columns; i++)
                        {
                            if (i == groupColumn)
                            {
                                infoResourceId = executer.Reader.GetString(groupColumn);
                                continue;
                            }
                            if (i == indexColumn)
                            {
                                partAccessField.Add(new XAttribute("index", executer.Reader.GetString(indexColumn)));
                                continue;
                            }
                            partAccessField.Add(new XElement(executer.Reader.GetName(i).ToUpper(), executer.Reader.GetString(i)));
                        }
                        var partFoundedAccessFields = partResources.Elements().Where((n) => n.Attribute("index").Value.Equals(infoResourceId)).SingleOrDefault();
                        if (partFoundedAccessFields != null)
                        {
                            partFoundedAccessFields.Element("RIGHTS").Add(partAccessField);
                        }
                    }
            }
            result.InformationResources = _fieldRepository.GetInformationalResourcesByClaim(result);
            return result;
        }

        private XElement ComputeRoot(XElement r)
        {
            if (r.Name == "DataSource")
            {
                string document = r.Attribute("documentKind").Value;
                _logger.Trace("Computing a root of {0}.", document);
                if (document.Equals("Claim"))
                {
                    XElement result = new XElement("ROOT");
                    ComputeCollection(r.Elements(), result);
                    return result;
                }
            }
            return null;
        }

        private void ComputeObject(XElement s, XElement d)
        {
            if (s.Name.Equals("Query"))
            {
                QueryExecuterWorkspace executerWork = new()
                {
                    Claim = null,
                    Logger = _logger,
                    SqlReqource = _sqlResource,
                };
                var dd = s.Attribute("arguments");
                if (dd != null)
                {
                    var args = d.XPathSelectElements(dd.Value).Select((el) => el.Value);
                    executerWork.Query = string.Format(s.Value, args.ToArray());
                }
                else
                {
                    executerWork.Query = string.Format(s.Value);
                }
                using (QueryExecuter executer = new QueryExecuter(executerWork))
                    while (executer.Reader.Read())
                    {
                        XElement accessField = new XElement("ITEM",
                            new XAttribute("index", executer.Reader.GetInt32(3)),
                            new XElement("NAME", executer.Reader.GetString(2)),
                            new XElement("Allowed", executer.Reader.GetString(4)),
                            new XElement("Denied", executer.Reader.GetString(5))
                        );
                        var infoResourceId = executer.Reader.GetInt32(1).ToString(); // ToString!?
                        var accField = d.Elements().Where((n) => n.Attribute("index").Value.Equals(infoResourceId)).SingleOrDefault();
                        if (accField != null)
                        {
                            accField.Element("RIGHTS").Add(accessField);
                        }
                    }
            }
        }

        private void ComputeCollection(IEnumerable<XElement> s, XElement d)
        {
            foreach (var e in s)
            {
                if (e.Name.Equals("Query"))
                {
                    ComputeObject(e, d);
                }
                if (e.Name.Equals("Collection"))
                {
                    var da = new XElement(e.Attribute("name").Value);
                    d.Add(da);
                    ComputeObject(e, da);
                }
                if (e.Name.Equals("Table"))
                {
                    var da = new XElement(e.Attribute("name").Value);
                    d.Add(da);
                    ComputeTable(e, da);
                }
            }
        }

        private void ComputeTable(XElement s, XElement d)
        {
            ;
        }
    }
}
