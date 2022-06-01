using DocAggregator.API.Core;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace DocAggregator.API.Infrastructure.OpenXMLProcessing
{
    /// <summary>
    /// Реализация <see cref="IDocument"/> на инструментах OpenOfficeXML.
    /// </summary>
    public class WordMLDocument : IDocument
    {
        public bool Finalized { get; set; }
        public string TemporaryDocumentPath { get; private set; }
        /// <summary>
        /// Поток, содержащий документ.
        /// </summary>
        public MemoryStream ResultStream { get; set; }
        /// <summary>
        /// OpenXML представление документа.
        /// </summary>
        public WordprocessingDocument Content { get; set; }
        /// <summary>
        /// XML представление главной части документа.
        /// </summary>
        public XDocument MainPart { get; set; }

        /// <summary>
        /// Инициализирует исходный документ по заданному пути.
        /// </summary>
        /// <param name="stream">Путь к исходному файлу.</param>
        public WordMLDocument(string path)
        {
            MemoryStream tempStream = new MemoryStream();
            //File.OpenRead(path).CopyTo(tempStream);
            TemporaryDocumentPath = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString() + ".docx");
            File.Copy(path, TemporaryDocumentPath);

            ResultStream = tempStream;
            Content = WordprocessingDocument.Open(TemporaryDocumentPath, true);

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
    }
}
