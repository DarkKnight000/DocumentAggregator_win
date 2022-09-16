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
        private readonly ModelBind _binds;
        private readonly SqlConnectionResource _sqlResource;

        private const string ROOT_TEMPLATE_NAME = "template";
        private const string ITEM_KEY_NAME = "key";

        public ClaimRepository(SqlConnectionResource sqlResource, TemplateMap templateMap, ModelBind modelBind, ILoggerFactory loggerFactory, IOptionsFactory optionsFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimRepository>();
            _templates = templateMap;
            _binds = modelBind;
            _sqlResource = sqlResource;
        }

        public Claim GetClaim(DocumentRequest req)
        {
            XDocument blockDocument = _binds.GetBind(req.Type);
            XElement partRoot = new XElement(req.Type, req.Args.Select((pair) => new XElement(pair.Key.ToLower(), pair.Value)));
            OracleConnection connection = QueryExecuter.BuildConnection(_sqlResource);
            connection.Open();
            QueryExecuterWorkspace executerWork = new()
            {
                Connection = connection,
                Logger = _logger,
                SqlReqource = _sqlResource,
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
            partRoot.Add(new XAttribute(ROOT_TEMPLATE_NAME, _templates.GetTemplate(req.Type.ToLower(), partRoot)));
            connection.Close();
            return new Claim() { Root = partRoot };
        }

        private void ExtractedQueryProcessing(XElement blockQuery, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            string argument = "";
            if (blockQuery.Attribute(DSS.arguments) != null)
            {
                argument = partRoot.XPathSelectElement(blockQuery.Attribute(DSS.arguments).Value.ToLower())?.Value;
            }
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockQuery.Value, argument))
                while (executer.Reader.Read())
                {
                    for (int i = 0; i < executer.Reader.FieldCount; i++)
                    {
                        partRoot.Add(new XElement(executer.Reader.GetName(i).ToLower(), executer.Reader.GetStringOrEmpty(i)));
                    }
                }
        }

        private void ExtractedCollectionProcessing(XElement blockCollection, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            var name = blockCollection.Attribute(DSS.name)?.Value.ToLower();
            string argument = "";
            if (blockCollection.Attribute(DSS.arguments) != null)
            {
                argument = partRoot.XPathSelectElement(blockCollection.Attribute(DSS.arguments).Value.ToLower())?.Value;
            }
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockCollection.Value, argument))
                while (executer.Reader.Read())
                {
                    int columns = executer.Reader.FieldCount;
                    int indexColumn = -1, valColumn = -1;
                    for (int i = 0; i < columns; i++)
                    {
                        if (executer.Reader.GetName(i).ToLower().Equals(blockCollection.Attribute(DSS.itemIndexColumn)?.Value.ToLower()))
                        {
                            indexColumn = i;
                        }
                        if (executer.Reader.GetName(i).ToLower().Equals(blockCollection.Attribute(DSS.valColumn)?.Value.ToLower()))
                        {
                            valColumn = i;
                        }
                    }
                    string nodeKey;
                    var node = new XElement(name);
                    if (valColumn == -1)
                    {
                        for (int i = 0; i < columns; i++)
                        {
                            if (i == indexColumn)
                            {
                                nodeKey = executer.Reader.GetStringOrEmpty(indexColumn);
                                node.Add(new XAttribute(ITEM_KEY_NAME, nodeKey));
                                continue;
                            }
                            else
                            {
                                node.Add(new XElement(executer.Reader.GetName(i).ToLower(), executer.Reader.GetStringOrEmpty(i)));
                            }    
                        }
                    }
                    else
                    {
                        for (int i = 0; i < columns; i++)
                        {
                            if (i == indexColumn)
                            {
                                nodeKey = executer.Reader.GetStringOrEmpty(indexColumn);
                                node.Add(new XAttribute(ITEM_KEY_NAME, nodeKey));
                                continue;
                            }
                            if (i == valColumn)
                            {
                                node.Value = executer.Reader.GetStringOrEmpty(valColumn);
                            }
                        }
                    }
                    partRoot.Add(node);
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
                    listOfLines.Add(executer.Reader.GetString(indexColumn)); // TEST: If index is null
                    var partItem = new XElement(blockTable.Attribute(DSS.name)?.Value.ToLower());
                    for (int i = 0; i < columns; i++)
                    {
                        if (i == indexColumn)
                        {
                            partItem.Add(new XAttribute(ITEM_KEY_NAME, executer.Reader.GetStringOrEmpty(indexColumn)));
                            continue;
                        }
                        partItem.Add(new XElement(executer.Reader.GetName(i).ToLower(), executer.Reader.GetStringOrEmpty(i)));
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
                                infoResourceId = executer.Reader.GetStringOrEmpty(groupColumn);
                                continue;
                            }
                            if (i == indexColumn)
                            {
                                partAccessField.Add(new XAttribute(ITEM_KEY_NAME, executer.Reader.GetStringOrEmpty(indexColumn)));
                                continue;
                            }
                            partAccessField.Add(new XElement(executer.Reader.GetName(i).ToLower(), executer.Reader.GetStringOrEmpty(i)));
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
