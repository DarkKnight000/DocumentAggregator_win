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
                XElement innerContent;
                switch (insert.Kind)
                {
                    case InsertKind.CheckMark:
                        innerContent = sdt.Element(W.sdtContent).Element(W.p) ?? sdt.Element(W.sdtContent).Element(W.tc);
                        if (innerContent == null)
                        {
                            continue;
                        }
                        innerContent.Descendants(W.t).Single().Value = insert.ReplacedCheckmark.Value ? "☒" : "☐";
                        break;
                    default:
                        innerContent = sdt.Element(W.sdtContent).Element(W.p) ?? sdt.Element(W.sdtContent).Element(W.tc);
                        if (innerContent == null)
                        {
                            continue;
                        }
                        innerContent.Descendants(W.t).Single().Value = insert.ReplacedText;
                        break;
                }
                sdt.ReplaceWith(innerContent);
                insert.AssociatedChunk = innerContent;
            }
        }
    }
}
