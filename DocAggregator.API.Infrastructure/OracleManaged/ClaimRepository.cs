using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Oracle.ManagedDataAccess.Client;
using System;
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

        /// <summary>
        /// Загружает все доступные данные по заявке используя модель запрашиваемого документа,
        /// информацию из базы данных и карту сопоставлений шаблонов.
        /// </summary>
        /// <param name="req">Запрос генератора.</param>
        /// <returns>Объект документа.</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="SolvableException"/>
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
            string argument = GetArgument(blockQuery, partRoot);
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
            string argument = GetArgument(blockCollection, partRoot);
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
            var argument = GetArgument(blockFor, partRoot);
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
                    try
                    {
                        listOfLines.Add(executer.Reader.GetString(indexColumn));
                    }
                    catch (IndexOutOfRangeException ex) // If index is null
                    {
                        RepositoryExceptionHelper.ThrowIndexOfForElementFailure(ex);
                    }
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

        /// <summary>
        /// Получает атрибут аргумента заданного блока
        /// и получает значения элементов полей заданных в запросе значения атрибута.
        /// </summary>
        /// <param name="block">Элемент, содержащий (или нет) аргументы.</param>
        /// <param name="partRoot">Корень собираемой модели документа.</param>
        /// <returns>Результат запроса из атрибута аргументов или <see cref="string.Empty"/>.</returns>
        private string GetArgument(XElement block, XElement partRoot)
        {
            var argumentQuery = block.Attribute(DSS.arguments);
            return argumentQuery == null ? string.Empty :
                partRoot.XPathSelectElement(argumentQuery.Value.ToLower())?.Value ?? string.Empty;
        }
    }
}
