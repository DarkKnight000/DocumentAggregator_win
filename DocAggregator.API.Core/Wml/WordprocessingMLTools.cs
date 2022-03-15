using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DocAggregator.API.Core.Wml
{
    public static class WordprocessingMLTools
    {
        public static IEnumerable<Insert> FindInserts(XDocument document)
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
                    continue; // TODO: Log this?
                }
                string alias = properties.Element(W.alias)?.Attribute(W.val)?.Value;
                if (alias == null)
                {
                    continue; // TODO: Log this?
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
                    continue; // TODO: Log this?
                }
                yield return new Insert(alias, detectedKind.Value) { AssociatedChunk = sdt };
            }
        }

        public static void SetInserts(params Insert[] inserts)
        {
            foreach (Insert insert in inserts)
            {
                XElement sdt = insert.AssociatedChunk as XElement;
                XElement innerContent = sdt.Element(W.sdtContent);
                XElement cell = innerContent.Element(W.tc);
                if (cell != null)
                {   // We are inside a table cell.
                    Test(sdt, cell, cell.Element(W.p), insert);
                    continue;
                }
                XElement par = innerContent.Element(W.p);
                if (par != null)
                {   // We are in a paragraph.
                    Test(sdt, par, par, insert);
                    continue;
                }
                XElement run = innerContent.Element(W.r);
                if (run != null)
                {
                    Test(sdt, run, run, insert);
                    continue;
                }
                // TODO: Log, there are no text data.
            }
        }

        public static void Test(XElement sdt, XElement innerContent, XElement textContainer, Insert insert)
        {
            if (textContainer == null)
            {
                return;
            }
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    textContainer.Descendants(W.t).Single().Value = insert.ReplacedCheckmark.Value ? "☒" : "☐";
                    break;
                default:
                    textContainer.Descendants(W.rStyle).SingleOrDefault()?.Remove();
                    textContainer.Descendants(W.t).Single().Value = insert.ReplacedText;
                    break;
            }
            sdt?.ReplaceWith(innerContent);
            insert.AssociatedChunk = innerContent;
        }
    }
}
