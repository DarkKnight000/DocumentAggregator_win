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
            XDocument blockDocument = XDocument.Load(filePath);
            //XElement altRoot = ComputeRoot(desc.Root);
            int typeID = -1, registerSystemID = -1;
            XElement partRoot = new XElement("ROOT", new XElement("ID", id));
            OracleConnection connection = QueryExecuter.BuildConnection(_sqlResource);
            connection.Open();
            QueryExecuterWorkspace executerWork = new()
            {
                Connection = connection,
                Logger = _logger,
                SqlReqource = _sqlResource,
            };
            executerWork.Query = string.Format(_sqlResource.GetStringByName("Q_HRDClaimSystemID_ByRequest"), id);
            using (QueryExecuter executer = new QueryExecuter(executerWork))
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
            partRoot.Add(new XAttribute("template", template));
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
            int mode = -1;
            var argument = partRoot.XPathSelectElement(blockQuery.Attribute(DSS.arguments).Value)?.Value;
            executerWork.Query = string.Format(blockQuery.Value, argument);
            if (blockQuery.Attributes().Any((a) => a.Name.Equals(DSS.itemIndexColumn)) &&
                blockQuery.Attributes().Any((a) => a.Name.Equals(DSS.valColumn)))
            {
                mode = 1;
            }
            else
            {
                mode = 2;
            }
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                switch (mode)
                {
                    case 1:
                        while (executer.Reader.Read())
                        {
                            var nodeName = executer.Reader.IsDBNull(0) ? "-" : executer.Reader.GetString(0);
                            partRoot.Add(new XElement("ITEM", new XAttribute("key", nodeName), executer.Reader.IsDBNull(1) ? string.Empty : executer.Reader.GetString(1)));
                        }
                        break;
                    case 2:
                        while (executer.Reader.Read())
                        {
                            for (int i = 0; i < executer.Reader.FieldCount; i++)
                            {
                                partRoot.Add(new XElement(executer.Reader.GetName(i), executer.Reader.IsDBNull(i) ? string.Empty : executer.Reader.GetString(i)));
                            }
                        }
                        break;
                }
        }

        private void ExtractedCollectionProcessing(XElement blockCollection, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            var blockQuery = blockCollection.Element(DSS.Query);
            XElement partFields = new XElement(blockCollection.Attribute(DSS.name)?.Value.ToUpper());
            partRoot.Add(partFields);
            ExtractedQueryProcessing(blockQuery, partFields, executerWork);
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
            var containerName = blockGet.Attribute(DSS.name)?.Value.ToUpper();
            var argument = partRoot.XPathSelectElement(blockFor.Attribute(DSS.arguments).Value)?.Value;
            executerWork.Query = string.Format(blockFor.Value, argument);
            XElement partResources = new XElement(blockTable.Attribute(DSS.name)?.Value.ToUpper());
            partRoot.Add(partResources);
            List<string> listOfLines = new List<string>();
            using (QueryExecuter executer = new QueryExecuter(executerWork))
                while (executer.Reader.Read())
                {
                    int columns = executer.Reader.FieldCount;
                    int indexColumn = -1;
                    for (int i = 0; i < columns; i++)
                    {
                        if (executer.Reader.GetName(i).Equals(blockFor.Attribute(DSS.itemIndexColumn).Value.ToUpper()))
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
                            partItem.Add(new XAttribute("key", executer.Reader.GetString(indexColumn)));
                            continue;
                        }
                        partItem.Add(new XElement(executer.Reader.GetName(i).ToUpper(), executer.Reader.GetString(i)));
                    }
                    partResources.Add(partItem);
                    partResources.Elements().Last().Add(new XElement(containerName));
                }
            var blockTableQuery = blockGet.Element(DSS.Query);
            foreach (var lineArg in listOfLines)
            {
                executerWork.Query = string.Format(blockTableQuery.Value, lineArg, argument);
                using (QueryExecuter executer = new QueryExecuter(executerWork))
                    while (executer.Reader.Read())
                    {
                        XElement partAccessField = new XElement("ITEM");
                        string infoResourceId = null;
                        int columns = executer.Reader.FieldCount;
                        int indexColumn = -1, groupColumn = -1;
                        for (int i = 0; i < columns; i++)
                        {
                            if (executer.Reader.GetName(i).Equals(blockTableQuery.Attribute(DSS.itemIndexColumn).Value.ToUpper()))
                            {
                                indexColumn = i;
                            }
                            if (executer.Reader.GetName(i).Equals(blockTableQuery.Attribute(DSS.groupColumn).Value.ToUpper()))
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
                                partAccessField.Add(new XAttribute("key", executer.Reader.GetString(indexColumn)));
                                continue;
                            }
                            partAccessField.Add(new XElement(executer.Reader.GetName(i).ToUpper(), executer.Reader.GetString(i)));
                        }
                        var partFoundedAccessFields = partResources.Elements().Where((n) => n.Attribute("key").Value.Equals(infoResourceId)).SingleOrDefault();
                        if (partFoundedAccessFields != null)
                        {
                            partFoundedAccessFields.Element(containerName).Add(partAccessField);
                        }
                    }
            }
        }
    }
}
