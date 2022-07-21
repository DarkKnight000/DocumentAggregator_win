using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Xml.Linq;

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
            int typeID = -1, registerSystemID = -1;
            XElement root = new XElement("ROOT", new XElement("ID", id));
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
                typeID = executer.Reader.GetInt32(0);
                root.Add(new XElement("TYPEID", typeID));
                registerSystemID = executer.Reader.GetInt32(1);
                root.Add(new XElement("SYSTEMID", registerSystemID));
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
            root.Add(new XElement("TEMPLATE", template));
            Claim result = new()
            {
                ID = id,
                TypeID = typeID,
                Root = root,
                SystemID = registerSystemID,
                Template = template,
                DbConnection = connection,
            };
            executerWork.Claim = result;
            executerWork.Query = string.Format(desc.Root.Element("Collection").Element("SqlQuery").Value, result.ID);
            XElement fields = new XElement("ATTRIBUTES");
            root.Add(fields);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    var nodeName = executer.Reader.IsDBNull(0) ? "-" : executer.Reader.GetString(0);
                    fields.Add(new XElement("ITEM", new XAttribute("index", nodeName), executer.Reader.IsDBNull(1) ? string.Empty : executer.Reader.GetString(1)));
                }
            executerWork.Query = string.Format(desc.Root.Elements("Collection").Where((n) => n.Attribute("name").Value.Equals("Custom")).Single().Element("SqlQuery").Value, result.ID);
            XElement cust = new XElement("CUSTOM");
            root.Add(cust);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    for (int i = 0; i < executer.Reader.FieldCount; i++)
                    {
                        cust.Add(new XElement(executer.Reader.GetName(i), executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i)));
                    }
                }
            result.ClaimFields = _fieldRepository.GetFiledListByClaimId(result, false);
            executerWork.Query = string.Format(desc.Root.Elements("Collection").Where((n) => n.Attribute("name").Value.Equals("Resources")).Single().Element("SqlQuery").Value, result.ID);
            XElement res = new XElement("RESOURCES");
            root.Add(res);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    AccessRightStatus stat = AccessRightStatus.NotMentioned;
                    if (!executer.Reader.IsDBNull(4))
                    {
                        switch (executer.Reader.GetString(4))
                        {
                            case "0":
                                stat = AccessRightStatus.Denied;
                                break;
                            case "1":
                                stat = AccessRightStatus.Allowed;
                                break;
                            default:
                                break;
                        }
                    }
                    XElement accessField = new XElement("ITEM",
                        new XAttribute("index", executer.Reader.GetInt32(3)),
                        new XElement("Name", executer.Reader.GetString(2)),
                        new XElement("Status", stat)
                    );
                    var infoResourceId = executer.Reader.GetInt32(1).ToString(); // ToString!?
                    var accField = res.Elements().Where((n) => n.Attribute("index").Value.Equals(infoResourceId)).SingleOrDefault();
                    if (accField != null)
                    {
                        accField.Element("AccessRightFields").Add(accessField);
                    }
                    else
                    {
                        res.Add(new XElement("ITEM",
                            new XAttribute("index", infoResourceId),
                            new XElement("Name", executer.Reader.GetString(0)),
                            new XElement("AccessRightFields", accessField)
                        ));
                    }
                }
            result.InformationResources = _fieldRepository.GetInformationalResourcesByClaim(result);
            return result;
        }
    }
}
