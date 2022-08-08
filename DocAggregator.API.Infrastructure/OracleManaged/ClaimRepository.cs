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

        private const string ROOT_TEMPLATE_NAME = "template";
        private const string ITEM_KEY_NAME = "key";

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
            XDocument blockDocument = XDocument.Load(filePath);
            //XElement altRoot = ComputeRoot(desc.Root);
            int typeID = -1, registerSystemID = -1;
            XElement partRoot = new XElement("claim", new XElement("id", id));
            OracleConnection connection = QueryExecuter.BuildConnection(_sqlResource);
            connection.Open();
            QueryExecuterWorkspace executerWork = new()
            {
                Connection = connection,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(_sqlResource.GetStringByName("Q_HRDClaimSystemID_ByRequest"), id))
            {
                executer.Reader.Read();
                //Setting Initials
                typeID = executer.Reader.GetInt32(0);
                registerSystemID = executer.Reader.GetInt32(1);
                if (executer.Reader.Read())
                {
                    _logger.Error("When processing a claim with id {0} two or more system bindings were found. The first two: [{1}, {2}] and [{3}, {4}].",
                        id, typeID, registerSystemID, executer.Reader.GetInt32(0), executer.Reader.GetInt32(1));
                    throw new System.Exception("One claim had two or more related informational systems. See the log for more info.");
                }
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
            partRoot.Add(new XAttribute(ROOT_TEMPLATE_NAME, template));
            Claim result = new()
            {
                ID = id,
                TypeID = typeID,
                Root = partRoot,
                SystemID = registerSystemID,
                Template = template,
            };
            foreach(var block in blockDocument.Root.Elements())
            {
                if (block.Name.Equals(DSS.Query))
                {
                    ExtractedQueryProcessing(block, partRoot, executerWork);
                }
                if (block.Name.Equals(DSS.Collection))
                {
                    ExtractedCollectionProcessing(block, partRoot, executerWork);
                }
                if (block.Name.Equals(DSS.Table))
                {
                    ExtractedTableProcessing(block, partRoot, executerWork);
                }
            }
            //result.ClaimFields = _fieldRepository.GetFiledListByClaimId(result, false);
            //result.InformationResources = _fieldRepository.GetInformationalResourcesByClaim(result);
            connection.Close();
            return result;
        }

        private void ExtractedQueryProcessing(XElement blockQuery, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            var argument = partRoot.XPathSelectElement(blockQuery.Attribute(DSS.arguments).Value.ToLower())?.Value;
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockQuery.Value, argument))
                while (executer.Reader.Read())
                {
                    for (int i = 0; i < executer.Reader.FieldCount; i++)
                    {
                        partRoot.Add(new XElement(executer.Reader.GetName(i).ToLower(), executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i)));
                    }
                }
        }

        private void ExtractedCollectionProcessing(XElement blockCollection, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            var name = blockCollection.Attribute(DSS.name)?.Value.ToLower();
            var argument = partRoot.XPathSelectElement(blockCollection.Attribute(DSS.arguments).Value.ToLower())?.Value;
            if (blockCollection.Attributes().Any((a) => a.Name.Equals(DSS.itemIndexColumn)) &&
                blockCollection.Attributes().Any((a) => a.Name.Equals(DSS.valColumn)))
            {
                ; // ?
            }
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockCollection.Value, argument))
                while (executer.Reader.Read())
                {
                    var nodeName = executer.Reader.IsDBNull(0) ? "-" : executer.Reader.GetString(0);
                    partRoot.Add(new XElement(name, new XAttribute(ITEM_KEY_NAME, nodeName), executer.Reader.IsDBNull(1) ? string.Empty : executer.Reader.GetString(1)));
                }
        }

        /// <summary>
        /// Temporary implementation of the part that processing a table with these For and Get elements inside.
        /// </summary>
        /// <param name="blockTable">The Table element.</param>
        /// <param name="partRoot">The element in which i put the result of the computations.</param>
        /// <param name="executerWork">Some executer workspace to use with the executor.</param>
        private void ExtractedTableProcessing(XElement blockTable, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            var blockFor = blockTable.Element(DSS.For);
            var blockGet = blockTable.Element(DSS.Get);
            var containerName = blockGet.Attribute(DSS.name)?.Value.ToLower();
            var argument = partRoot.XPathSelectElement(blockFor.Attribute(DSS.arguments).Value.ToLower())?.Value;
            List<string> listOfLines = new List<string>();
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockFor.Value, argument))
                while (executer.Reader.Read())
                {
                    int columns = executer.Reader.FieldCount;
                    int indexColumn = -1;
                    for (int i = 0; i < columns; i++)
                    {
                        if (executer.Reader.GetName(i).ToLower().Equals(blockFor.Attribute(DSS.itemIndexColumn).Value.ToLower()))
                        {
                            indexColumn = i;
                        }
                    }
                    listOfLines.Add(executer.Reader.GetString(0));
                    var partItem = new XElement(blockTable.Attribute(DSS.name)?.Value.ToLower());
                    for (int i = 0; i < columns; i++)
                    {
                        if (i == indexColumn)
                        {
                            partItem.Add(new XAttribute(ITEM_KEY_NAME, executer.Reader.GetString(indexColumn)));
                            continue;
                        }
                        partItem.Add(new XElement(executer.Reader.GetName(i).ToLower(), executer.Reader.GetString(i)));
                    }
                    partRoot.Add(partItem);
                }
            var blockTableQuery = blockGet;
            foreach (var lineArg in listOfLines)
            {
                using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockTableQuery.Value, lineArg, argument))
                    while (executer.Reader.Read())
                    {
                        XElement partAccessField = new XElement(containerName);
                        string infoResourceId = null;
                        int columns = executer.Reader.FieldCount;
                        int indexColumn = -1, groupColumn = -1;
                        for (int i = 0; i < columns; i++)
                        {
                            if (executer.Reader.GetName(i).ToLower().Equals(blockTableQuery.Attribute(DSS.itemIndexColumn).Value.ToLower()))
                            {
                                indexColumn = i;
                            }
                            if (executer.Reader.GetName(i).ToLower().Equals(blockTableQuery.Attribute(DSS.groupColumn).Value.ToLower()))
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
                                partAccessField.Add(new XAttribute(ITEM_KEY_NAME, executer.Reader.GetString(indexColumn)));
                                continue;
                            }
                            partAccessField.Add(new XElement(executer.Reader.GetName(i).ToLower(), executer.Reader.GetString(i)));
                        }
                        var partFoundedAccessFields = partRoot.Elements(blockTable.Attribute(DSS.name)?.Value.ToLower()).Where((n) => n.Attribute(ITEM_KEY_NAME).Value.Equals(infoResourceId)).SingleOrDefault();
                        if (partFoundedAccessFields != null)
                        {
                            partFoundedAccessFields.Add(partAccessField);
                        }
                    }
            }
        }
    }
}
