using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DocAggregator.API.Core.Wml
{
    /// <summary>
    /// Предоставляет методы для обработки OOXML-узлов.
    /// </summary>
    /// <remarks>
    /// Содержит <see cref="ILogger"/> для отслеживания хода обработки.
    /// </remarks>
    public class WordprocessingMLEditor
    {
        ILogger _logger;

        public WordprocessingMLEditor(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Проводит поиск вставок по документу.
        /// </summary>
        /// <param name="document">OOXML документ.</param>
        /// <returns>Перечисление найденных вставок.</returns>
        public IEnumerable<Insert> FindInserts(XDocument document)
        {
            if (document == null || document.Root == null)
            {
                _logger.Trace("Будет возвращена пустая коллекция вставок.");
                yield break;
            }
            _logger.Debug("Получаем все не вложенные элементы управления.");
            var topLevelControls = from control in document.Root.DescendantsAndSelf(W.sdt)
                                   where !control.Ancestors(W.sdt).Any()
                                   select control;
            foreach (var sdt in topLevelControls)
            {
                InsertKind? detectedKind = null;
                /// Локальная функция позволяет проверить значение перед присвоением локальной переменной.
                void SetDetectedInsertKind(InsertKind insertKind)
                {
                    if (detectedKind != null)
                    {
                        throw new FormatException($"The WordprocessingML document part has a content control beloging simultaneously to {detectedKind} and {insertKind} kinds.");
                    }
                    detectedKind = insertKind;
                }
                _logger.Trace("Обработка повреждений структуры документа.");
                XElement properties = sdt.Element(W.sdtPr);
                if (properties == null)
                {
                    _logger.Warning("Current content control has no properties child element.");
                    continue;
                }
                string alias = properties.Element(W.alias)?.Attribute(W.val)?.Value;
                if (alias == null)
                {
                    _logger.Warning("Have found a content control with empty alias.");
                    continue;
                }
                _logger.Trace("Определение типа элемента управления содержимым.");
                if (properties.Element(W.text) != null)
                {
                    SetDetectedInsertKind(InsertKind.PlainText);
                }
                if (properties.Element(W14.checkbox) != null)
                {
                    SetDetectedInsertKind(InsertKind.CheckMark);
                }
                if (!detectedKind.HasValue)
                {
                    _logger.Trace("Проверка на таблицу.");
                    var table = sdt.Element(W.sdtContent).Element(W.tbl);
                    if (table != null)
                    {
                        yield return new FormInsert(_logger) { AssociatedChunk = sdt };
                    }
                    else
                    {
                        _logger.Warning("Have found a content control, but not its kind.");
                    }
                    continue;
                }
                yield return new Insert(alias, detectedKind.Value, _logger) { AssociatedChunk = sdt };
            }
        }

        /// <summary>
        /// Устанавливает значения вставок в структуру документа.
        /// </summary>
        /// <param name="inserts">Массив вставок, ассоциированных с участками документов.</param>
        public void SetInserts(params Insert[] inserts)
        {
            foreach (Insert insert in inserts)
            {
                XElement sdt = insert.AssociatedChunk as XElement;
                XElement innerContent = sdt.Element(W.sdtContent);
                XElement cell = innerContent.Element(W.tc);
                if (cell != null)
                {
                    _logger.Debug("Processing a table cell.");
                    ReplaceContentControl(sdt, cell, insert);
                    continue;
                }
                XElement par = innerContent.Element(W.p);
                if (par != null)
                {
                    _logger.Debug("Processing a paragraph.");
                    ReplaceContentControl(sdt, par, insert);
                    continue;
                }
                XElement run = innerContent.Element(W.r);
                if (run != null)
                {
                    _logger.Debug("Processing a text range.");
                    ReplaceContentControl(sdt, run, insert);
                    continue;
                }
                _logger.Warning("There are no text element in the content control.");
            }
        }

        /// <summary>
        /// Put the value of <paramref name="insert"/> into <paramref name="innerTextContainer"/> using text properties of <paramref name="sdt"/>
        /// then replace <paramref name="sdt"/> with <paramref name="innerTextContainer"/>.
        /// </summary>
        /// <param name="sdt">Content control.</param>
        /// <param name="innerTextContainer">The inner element of <see cref="W.sdtPr"/> element.</param>
        /// <param name="insert">Target value of the content control.</param>
        public void ReplaceContentControl(XElement sdt, XElement innerTextContainer, Insert insert)
        {
            if (innerTextContainer == null)
            {
                return;
            }
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    innerTextContainer.Descendants(W.t).Single().Value = insert.ReplacedCheckmark.Value ? "☒" : "☐";
                    break;
                default: // InsertKind.PlainText
                    innerTextContainer.Descendants(W.rStyle).SingleOrDefault()?.Remove();
                    XElement tagRunPr = sdt.Element(W.sdtPr)?.Element(W.rPr);
                    XElement run = innerTextContainer.DescendantsAndSelf(W.r).Single();
                    if (tagRunPr != null && run != null)
                    {
                        XElement runPr = run.Element(W.rPr);
                        if (runPr == null)
                        {
                            runPr = new XElement(W.rPr);
                            run.Add(runPr);
                        }
                        foreach (XElement targetRunPr in tagRunPr.Elements())
                        {
                            XElement clone = new XElement(targetRunPr);
                            XElement candidate = runPr.Element(targetRunPr.Name);
                            if (candidate == null)
                            {
                                runPr.Add(clone);
                            }
                            else
                            {
                                candidate.ReplaceWith(clone);
                            }
                        }
                    }
                    innerTextContainer.Descendants(W.t).Single().Value = insert.ReplacedText;
                    break;
            }
            sdt?.ReplaceWith(innerTextContainer);
            insert.AssociatedChunk = innerTextContainer;
        }
    }
}
