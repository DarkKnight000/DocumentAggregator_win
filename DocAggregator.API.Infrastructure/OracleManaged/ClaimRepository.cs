using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
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

            // ИЗМЕНИЛ 23.04.2024
            partRoot.Add(new XAttribute(ROOT_TEMPLATE_NAME, _templates.GetTemplate(req.Type.ToLower(), partRoot)));

            //_logger.Information("\npartRoot: " + partRoot.ToString());
            connection.Close();
            return new Claim() { Root = partRoot };
        }

        private void ExtractedQueryProcessing(XElement blockQuery, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            string argument = GetArgument(blockQuery, partRoot);
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockQuery.Value, argument))
                foreach (var line in executer.GetLines())
                {
                    partRoot.Add(line.Select(f => new XElement(f.ColumnName.ToLower(), f.Value)).ToArray());
                }
        }

        private void ExtractedCollectionProcessing(XElement blockCollection, XElement partRoot, QueryExecuterWorkspace executerWork)
        {
            var name = blockCollection.Attribute(DSS.name)?.Value.ToLower();
            string argument = GetArgument(blockCollection, partRoot);
            using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockCollection.Value, argument))
            {
                var headers = executer.GetHeaders();
                // TEST
                //_logger.Error("Look at this!");
                //_logger.Critical(new Exception("aaaaa"), "You've messed up!");
                // TEST'S END
                int indexColumn = headers.FirstIndexMatch(h => h.ToLower().Equals(blockCollection.Attribute(DSS.itemIndexColumn)?.Value.ToLower())),
                    valColumn = headers.FirstIndexMatch(h => h.ToLower().Equals(blockCollection.Attribute(DSS.valColumn)?.Value.ToLower()));
                string nodeKey;
                foreach (var line in executer.GetLines())
                {
                    var node = new XElement(name);
                    if (valColumn == -1)            // ИЗМЕНИЛ на 0, не помогло
                    {
                        if (indexColumn != -1)
                        {
                            nodeKey = line.ElementAt(indexColumn).Value;
                            node.Add(new XAttribute(ITEM_KEY_NAME, nodeKey));
                        }
                        node.Add(line.Where(
                                (_, index) => index != indexColumn
                            ).Select(
                                f => new XElement(f.ColumnName.ToLower(), f.Value)
                            ));

                        //_logger.Information("\nif valColumn = -1 valColumn + indexColumn : " + valColumn.ToString() + " + " + indexColumn.ToString());
                    }
                    else
                    {
                        if (indexColumn != -1)
                        {
                            nodeKey = line.ElementAt(indexColumn).Value;
                            node.Add(new XAttribute(ITEM_KEY_NAME, nodeKey));
                        }
                        node.Value = line.ElementAt(valColumn).Value;
                    }
                    partRoot.Add(node);
                    //_logger.Information("\nvalColumn + indexColumn : " + valColumn.ToString() + " + " + indexColumn.ToString());
                    //_logger.Information("\nnode: " + node.ToString());
                }
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
            {
                int indexColumn = executer.GetHeaders().FirstIndexMatch(h => h.ToLower().Equals(blockFor.Attribute(DSS.itemIndexColumn)?.Value.ToLower()));
                foreach (var line in executer.GetLines())
                {
                    try
                    {
                        listOfLines.Add(line.ElementAt(indexColumn).Value);
                    }
                    catch (IndexOutOfRangeException ex) // If index is null
                    {
                        RepositoryExceptionHelper.ThrowIndexOfForElementFailure(ex);
                    }
                    var partItem = new XElement(blockTable.Attribute(DSS.name)?.Value.ToLower());
                    partItem.Add(new XAttribute(ITEM_KEY_NAME, line.ElementAt(indexColumn)));

                    //_logger.Information("\npartItem: " + partItem.ToString());

                    partItem.Add(line.Where(
                            (_, index) => index != indexColumn
                        ).Select(
                            f => new XElement(f.ColumnName.ToLower(), f.Value)
                        ).ToArray());
                    partRoot.Add(partItem);

                }

            }

            var blockTableQuery = blockGet;
            foreach (var lineArg in listOfLines)
            {

                using (QueryExecuter executer = executerWork.GetExecuterForQuery(blockTableQuery.Value, lineArg, argument))
                {
                    string infoResourceId = null;
                    var headers = executer.GetHeaders();
                    int indexColumn = headers.FirstIndexMatch(h => h.ToLower().Equals(blockTableQuery.Attribute(DSS.itemIndexColumn).Value.ToLower())),
                        groupColumn = headers.FirstIndexMatch(h => h.ToLower().Equals(blockTableQuery.Attribute(DSS.groupColumn).Value.ToLower()));
                    foreach (var line in executer.GetLines())
                    {
                        infoResourceId = line.ElementAt(groupColumn).Value;
                        XElement partAccessField = new XElement(containerName);


                        if (partAccessField.Attribute(ITEM_KEY_NAME)==null)
                        {
                            partAccessField.Add(new XAttribute(ITEM_KEY_NAME, line.ElementAt(indexColumn).Value));

                            //_logger.Information("\npartAccessField: " + partAccessField.ToString());
                        }

                        partAccessField.Add(line.Where(
                                (_, index) => index != groupColumn && index != indexColumn
                            ).Select(
                                f => new XElement(f.ColumnName.ToLower(), f.Value)
                            ));
                        
                        var partFoundedAccessFields = partRoot.Elements(
                                blockTable.Attribute(DSS.name)?.Value.ToLower()
                            ).Where(
                                (n) => n.Attribute(ITEM_KEY_NAME).Value.Equals(infoResourceId)
                            ).SingleOrDefault();
                        
                        if (partFoundedAccessFields != null)
                        {
                            partFoundedAccessFields.Add(partAccessField);
                        }

                        //_logger.Information("\npartFoundedAccessFields: " + partFoundedAccessFields.ToString());
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
