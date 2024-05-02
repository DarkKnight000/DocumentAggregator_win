﻿using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        readonly ILogger _logger;

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
                //_logger.Trace("Будет возвращена пустая коллекция вставок.");
                return Enumerable.Empty<Insert>();
            }
            //_logger.Debug("Получаем все не вложенные элементы управления.");
            var topLevelControls = from control in document.Root.DescendantsAndSelf(W.sdt)
                                   where !control.Ancestors(W.sdt).Any()
                                   select control;
            return RecursiveDetectInserts(topLevelControls);
        }

        /// <summary>
        /// Проводит рекурсивный сбор вставок на основе элементов управления.
        /// </summary>
        /// <remarks>
        /// Максимально корректно поддерживаемый уровень рекурсии - 1,
        /// так как не подразумевается вкладывание контейнеров внутрь других контейнеров.
        /// </remarks>
        /// <param name="targets">Элементы управления содержимым.</param>
        /// <returns>Перечисление определённых на основе элементов вставок.</returns>
        private IEnumerable<Insert> RecursiveDetectInserts(IEnumerable<XElement> targets)
        {
            foreach (var sdt in targets)
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
                //_logger.Trace("Обработка повреждений структуры документа.");
                XElement properties = sdt.Element(W.sdtPr);
                if (properties == null)
                {
                    _logger.Warning("Current content control has no properties child element.");
                    continue;
                }
                string alias = properties.Element(W.alias)?.Attribute(W.val)?.Value;
                string tag = properties.Element(W.tag)?.Attribute(W.val)?.Value;
                if (alias == null)
                {
                    _logger.Warning("Have found a content control with empty alias.");
                    continue;
                }
                if (tag == null)
                {
                    _logger.Warning("Have found a content control with empty tag.");
                    continue;
                }
                //_logger.Trace("Определение типа элемента управления содержимым.");
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
                    //_logger.Trace("Проверка на строку таблицы.");
                    var tableRow = sdt.Element(W.sdtContent).Element(W.tr);
                    if (tableRow != null)
                    {
                        var result = new FormInsert(alias, tag, _logger) { AssociatedChunk = sdt };
                        //_logger.Debug("Получаем ВСЕ вложенные элементы управления.");
                        var innerLevelControls = tableRow.DescendantsAndSelf(W.sdt);
                        // WARNING: При дальнейшей вложенности может обнаружиться дублирование элементов,
                        // так как innerLevelControls будет содержать как элементы следующего в порядке уровня, так и остальных.
                        foreach (var insert in RecursiveDetectInserts(innerLevelControls))
                        {
                            result.FormFields.Add(insert);
                        }
                        yield return result;
                    }
                    else
                    {
                        _logger.Warning("Have found a content control, but not its kind.");
                    }
                    continue;
                }
                yield return new Insert(alias, tag, detectedKind.Value, _logger) { AssociatedChunk = sdt };
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
                XElement row = innerContent.Element(W.tr);
                if (row != null)
                {
                    //_logger.Debug("Processing a table row.");
                    ReplaceContentControl(sdt, row, insert);
                    continue;
                }
                XElement cell = innerContent.Element(W.tc);
                if (cell != null)
                {
                    //_logger.Debug("Processing a table cell.");
                    ReplaceContentControl(sdt, cell, insert);
                    continue;
                }
                XElement par = innerContent.Element(W.p);
                if (par != null)
                {
                    //_logger.Debug("Processing a paragraph.");
                    ReplaceContentControl(sdt, par, insert);
                    continue;
                }
                // checkbox и ещё что-то
                XElement run = innerContent.Element(W.r);
                if (run != null)
                {
                    //_logger.Debug("Processing a text range.");
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
                    if (insert.Required && insert.ReplacedCheckmark == null)
                    {
                        throw new SolvableValidationException(insert);
                    }

                    //_logger.Trace("before innerTextContainer.Descendants(W.t).Single().Value   " + (innerTextContainer.Descendants(W.t).Single().Value.ToString() == "☒" ? "1" : "0"));
                    //_logger.Trace("innerTextContainer   " + innerTextContainer);
                    innerTextContainer.Descendants(W.t).Single().Value = insert.ReplacedCheckmark.Value ? "☒" : "☐";
                    //_logger.Trace("insert.ReplacedCheckmark.Value   " + insert.ReplacedCheckmark.Value);
                    //_logger.Trace("innerTextContainer   " + innerTextContainer);
                    //_logger.Trace("innerTextContainer   " + innerTextContainer.Descendants(W.t).Single().Value + "     " + insert.ReplacedCheckmark.Value);
                    //_logger.Trace("after innerTextContainer.Descendants(W.t).Single().Value   " + (innerTextContainer.Descendants(W.t).Single().Value.ToString() == "☒" ? "1" : "0"));


                    break;
                case InsertKind.MultiField:
                    if (insert is FormInsert form)
                    {
                        if (insert.Required && !form.FormValues.Any())
                        {
                            throw new SolvableValidationException(form);
                        }
                        _logger.Trace("Deleting the initial row.");
                        innerTextContainer.Remove();
                        var generatedRows = new List<XElement>();
                        for (int line = 0; line < form.FormValues.Count; line++)
                        {
                            var controls = new Insert[form.FormFields.Count];
                            var rowCopy = new XElement(innerTextContainer);
                            _logger.Trace("Clonning the initial row.");
                            var innerControls = rowCopy.DescendantsAndSelf(W.sdt).ToArray();
                            var valuesLine = form.FormValues[line];
                            for (int control = 0; control < valuesLine.Count; control++)
                            {
                                var formControl = form.FormFields[control];
                                _logger.Debug("Regenerating inner Insert with local data.");
                                // TODO: Should I cut the assignment out of the function?
                                controls[control] = new Insert(formControl)
                                {
                                    ReplacedText = valuesLine[control],
                                    ReplacedCheckmark = formControl.Kind.Equals(InsertKind.CheckMark)
                                        ? bool.TryParse(valuesLine[control], out bool parsedResult) && parsedResult
                                        : null,
                                    AssociatedChunk = innerControls[control],
                                };
                            }
                            SetInserts(controls);
                            _logger.Trace("Add a new row in a list.");
                            generatedRows.Add(rowCopy);
                        }
                        _logger.Trace("Putting rows in a \"table\" content control.");
                        sdt?.ReplaceWith(generatedRows.ToArray());
                        insert.AssociatedChunk = innerTextContainer;
                        return;
                    }
                    else
                    {
                        _logger.Warning("Insert of the {0} wasn't a {1} instance.", InsertKind.MultiField, typeof(FormInsert));
                    }    
                    break;
                default: // InsertKind.PlainText
                    if (insert.Required && string.IsNullOrWhiteSpace(insert.ReplacedText))
                    {
                        throw new SolvableValidationException(insert);
                    }
                    if (insert.Tag != "-" && insert.Tag != "required" && !Regex.IsMatch(insert.ReplacedText, insert.Tag))
                    {
                        throw new SolvableValidationException(insert);
                    }
                    XElement run = null;
                    foreach (var extraRun in innerTextContainer.Descendants(W.r))
                    {
                        if (run == null)
                        {
                            run = extraRun;
                        }
                        else
                        {
                            if (XNode.DeepEquals(run.Element(W.rPr), extraRun.Element(W.rPr)))
                            {
                                run.Element(W.t).Value += extraRun.Element(W.t).Value;
                            }
                            else
                            {
                                throw new SolvableException(
                                    string.Format("The text container name of \"{0}\" has several ({1}) so called \"runs\" with different properties. In the {2}({3})({4}).",
                                        innerTextContainer.Name,
                                        innerTextContainer.Descendants(W.rStyle).Count(),
                                        insert.Kind, insert.OriginalMask, insert.Tag
                                    ),
                                    "Remove the affected content control and create it again.");
                            }
                        }
                    }
                    innerTextContainer.Descendants(W.r).Skip(1).Remove();
                    innerTextContainer.Descendants(W.rStyle).FirstOrDefault()?.Remove();
                    XElement tagRunPr = sdt.Element(W.sdtPr)?.Element(W.rPr);
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
                    var lines = insert.ReplacedText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length < 2)
                    {
                        innerTextContainer.Descendants(W.t).Single().Value = insert.ReplacedText;
                    }
                    else
                    {
                        var text = new List<XElement>(lines.Length * 2 - 1);
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (i != 0)
                            {
                                text.Add(new XElement(W.br));
                            }
                            text.Add(new XElement(W.t, lines[i]));
                        }
                        innerTextContainer.Descendants(W.t).Single().ReplaceWith(text);
                    }
                    break;
            }
            sdt?.ReplaceWith(innerTextContainer);
            insert.AssociatedChunk = innerTextContainer;
        }
    }
}
