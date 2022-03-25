﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DocAggregator.API.Core.Wml
{
    public class WordprocessingMLEditor
    {
        ILogger _logger;

        public WordprocessingMLEditor(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<Insert> FindInserts(XDocument document)
        {
            if (document == null || document.Root == null)
            {
                yield break;
            }
            foreach (var sdt in document.Root.DescendantsAndSelf(W.sdt))
            {
                InsertKind? detectedKind = null;
                void TrySetDetectedInsertKind(InsertKind insertKind)
                {
                    if (detectedKind != null)
                    {
                        throw new FormatException($"The WordprocessingML document part has a content control beloging simultaneously to {detectedKind} and {insertKind} kinds.");
                    }
                    detectedKind = insertKind;
                }
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
                if (properties.Element(W.text) != null)
                {
                    TrySetDetectedInsertKind(InsertKind.PlainText);
                }
                if (properties.Element(W14.checkbox) != null)
                {
                    TrySetDetectedInsertKind(InsertKind.CheckMark);
                }
                if (!detectedKind.HasValue)
                {
                    _logger.Warning("Have found a content control, but not its kind.");
                    continue;
                }
                yield return new Insert(alias, detectedKind.Value, _logger) { AssociatedChunk = sdt };
            }
        }

        public void SetInserts(params Insert[] inserts)
        {
            foreach (Insert insert in inserts)
            {
                XElement sdt = insert.AssociatedChunk as XElement;
                XElement innerContent = sdt.Element(W.sdtContent);
                XElement cell = innerContent.Element(W.tc);
                if (cell != null)
                {   // We are inside a table cell.
                    ReplaceContentControl(sdt, cell, insert);
                    continue;
                }
                XElement par = innerContent.Element(W.p);
                if (par != null)
                {   // We are in a paragraph.
                    ReplaceContentControl(sdt, par, insert);
                    continue;
                }
                XElement run = innerContent.Element(W.r);
                if (run != null)
                {
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
                default:
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
