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
            InsertKind? detectedKind = null;
            void SetKind(InsertKind insertKind)
            {
                if (detectedKind != null)
                {
                    throw new FormatException($"The WordprocessingML document part has a content control beloging simultaneously to {detectedKind} and {insertKind} kinds.");
                }
                detectedKind = insertKind;
            }

            if (document == null || document.Root == null)
            {
                yield break;
            }
            foreach (var sdt in document.Root.DescendantsAndSelf(W.sdt))
            {
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
                    SetKind(InsertKind.PlainText);
                }
                if (properties.Element(W14.checkbox) != null)
                {
                    SetKind(InsertKind.CheckMark);
                }
                if (!detectedKind.HasValue)
                {
                    continue; // TODO: Log this?
                }
                yield return new Insert(alias, detectedKind.Value) { AssociatedChunk = sdt };
            }
        }
    }
}
