using DocAggregator.API.Core;
using DocAggregator.API.Core.Wml;
using DocumentFormat.OpenXml.Packaging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DocAggregator.API.Infrastructure.OpenXMLProcessing
{
    /// <summary>
    /// Реализация <see cref="IDocument"/> на инструментах OpenOfficeXML.
    /// </summary>
    public class WordMLDocument : IDocument
    {
        /// <summary>
        /// Временный путь к документу.
        /// </summary>
        public string ResultPath { get; set; }
        /// <summary>
        /// OpenXML представление документа.
        /// </summary>
        public WordprocessingDocument Content { get; set; }
        /// <summary>
        /// XML представление главной части документа.
        /// </summary>
        public XDocument MainPart { get; set; }

        public WordMLDocument(string path)
        {
            ResultPath = path;
            Content = WordprocessingDocument.Open(path, true);

            MainPart = LoadPart(Content.MainDocumentPart);
        }

        /// <summary>
        /// Загружает часть документа в XML узел типа <see cref="XDocument"/>.
        /// </summary>
        /// <param name="source">Часть документа WordprocessingML.</param>
        /// <returns>XML представление этой части.</returns>
        private XDocument LoadPart(OpenXmlPart source)
        {
            if (source == null)
            {
                return null;
            }
            var part = source.Annotation<XDocument>();
            if (part != null)
            {
                return part;
            }
            using (var str = source.GetStream())
            using (var streamReader = new StreamReader(str))
            using (var xr = XmlReader.Create(streamReader))
            {
                part = XDocument.Load(xr);
            }
            return part;
        }

        internal IEnumerable<Insert> GetInserts()
        {
            var root = MainPart.Root;
            // Among all the conent control nodes ..
            return from sdt in root.DescendantsAndSelf(W.sdt)
                   select new Insert(
                       // .. get properties
                       sdt.Element(W.sdtPr)
                          // .. get the title of the content control
                          .Element(W.alias)
                          // .. get its string value
                          .Attribute(W.val)
                          // .. as a mask for the insert.
                          .Value /*?? string.Empty*/,
                       sdt.Element(W.sdtPr) // Depends on properties
                          // .. choose appropriate insert kind.
                          .Element(W14.checkbox) == null ? InsertKind.PlainText : InsertKind.CheckMark)
                   // Save the control XML node.
                   { AssociatedChunk = sdt };
        }

        internal void SetInserts(IEnumerable<Insert> inserts)
        {
            foreach (var insert in inserts)
            {
                var sdt = insert.AssociatedChunk as XElement;
                switch (insert.Kind)
                {
                    case InsertKind.CheckMark:
                        // Find properties of a content control.
                        var sdtPr = sdt.Element(W.sdtPr);
                        if (sdtPr != null)
                        {
                            var checkBox = sdtPr.Element(W14.checkbox);
                            if (checkBox != null)
                            {
                                checkBox.Element(W14.check).Attribute(W14.val).Value = insert.ReplacedCheckmark.Value ? "1" : "0";
                                sdt.Elements().Where(e => e.DescendantsAndSelf(W.t).Any()).First().Value = insert.ReplacedCheckmark.Value ? "☒" : "☐";
                            }
                        }
                        break;
                    default: // InsertKind.PlainText
                        // Find a content of a content control.
                        var sdtContent = sdt.Element(W.sdtContent);
                        if (sdtContent == null)
                        {
                            sdt.Add(new XElement(W.sdtContent, new XElement(W.p), new XElement(W.r, new XElement(W.t, insert.ReplacedText))));
                        }
                        else
                        {
                            var elementsWithText = sdtContent.Elements() // Get all child elements
                                                                         // .. that contains text nodes and doesn't contain other content controls.
                                                             .Where(e => e.DescendantsAndSelf(W.t).Any() && !e.DescendantsAndSelf(W.sdt).Any())
                                                             .ToList();
                            var firstContentElementWithText = elementsWithText.FirstOrDefault(d => d.DescendantsAndSelf(W.t).Any());
                            if (firstContentElementWithText == null)
                            {
                                if (sdtContent.Elements(W.p).Any())
                                {
                                    sdtContent.Element(W.p).Add(new XElement(W.r, new XElement(W.t, insert.ReplacedText)));
                                }
                                else
                                {
                                    sdtContent.Add(new XElement(W.p), new XElement(W.r, new XElement(W.t, insert.ReplacedText)));
                                }
                            }
                            else
                            {
                                var firstTextElement = firstContentElementWithText
                                    .Descendants(W.t)
                                    .First();
                                firstTextElement.Value = insert.ReplacedText;

                                var firstElementAncestors = firstTextElement.AncestorsAndSelf().ToList();
                                foreach (var descendants in elementsWithText.DescendantsAndSelf().ToList())
                                {
                                    if (!firstElementAncestors.Contains(descendants) && descendants.DescendantsAndSelf(W.t).Any())
                                    {
                                        descendants.Remove();
                                    }
                                }

                                var contentReplacementElement = new XElement(firstContentElementWithText);
                                firstContentElementWithText.AddAfterSelf(contentReplacementElement);
                                firstContentElementWithText.Remove();
                            }
                        }
                        break;
                }
            }
        }
    }
}
