using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;

namespace DocAggregator.API.Infrastructure.OfficeInterop
{
    public class WordService : IEditorService, IDisposable
    {
        ILogger _logger;
        Word.Application app;

        public string TemplatesDirectory
        {
            get => _templatesDirectory;
            set
            {
                _templatesDirectory = Path.GetFullPath(value);
            }
        }
        private string _templatesDirectory;

        public string TemporaryOutputDirectory
        {
            get => _temporaryOutputDirectory;
            set
            {
                _temporaryOutputDirectory = Path.GetFullPath(value);
            }
        }
        private string _temporaryOutputDirectory;

        private bool disposedValue;

        public WordService(ILogger logger)
        {
            _logger = logger;
            // Очистка кучи для предупреждения COM+ ошибки HRESULT: 0x80080005 (не работает)
            GC.Collect();
            app = new Word.Application();
        }

        public IDocument OpenTemplate(string path)
        {
            WordDocument result = new WordDocument();
            string file = Path.Combine(TemplatesDirectory, path);
            if (!File.Exists(file))
            {
                _logger.Error("Template hasn't found. Template: {0}", file);
                throw new FileNotFoundException("Шаблон не найден.", file);
            }
            object template = file;
            object newTemplate = false;
            object documentType = Word.WdNewDocumentType.wdNewBlankDocument;
            object visible = false;
            Word.Document doc = null;
            doc = app.Documents.Add(ref template, ref newTemplate, ref documentType, ref visible);
            result.Body = doc;
            return result;
        }

        public IEnumerable<Insert> GetInserts(IDocument document)
        {
            var wordDocument = (WordDocument)document;
            if (wordDocument.Body == null)
            {
                _logger.Error("The document has no body.");
                yield break;
            }
            var range = wordDocument.Body.Range(wordDocument.Body.Content.Start, wordDocument.Body.Content.End);
            foreach (Word.ContentControl control in range.ContentControls)
            {
                _logger.Debug($"Register content control {control.Type} with a Title \"{control.Title}\".");
                if (control.Title == null)
                {
                    _logger.Warning("The Title is null! Skipped.");
                    continue;
                }
                InsertKind kind = InsertKind.Unknown;
                switch (control.Type)
                {
                    case Word.WdContentControlType.wdContentControlCheckBox:
                        kind = InsertKind.CheckMark;
                        break;
                    case Word.WdContentControlType.wdContentControlText:
                        kind = InsertKind.PlainText;
                        break;
                    default:
                        _logger.Warning("Unknown control type!");
                        break;
                }
                yield return new Insert(control.Title, kind) { AssociatedChunk = control };
            }
        }

        public void SetInserts(IDocument document, IEnumerable<Insert> inserts)
        {
            foreach (Insert insert in inserts)
            {
                _logger.Debug($"Putting \"{insert.ReplacedText ?? insert.ReplacedCheckmark.ToString()}\" in the control with tag \"{insert.OriginalMask}\".");
                Word.ContentControl control = insert.AssociatedChunk as Word.ContentControl;
                if (control == null)
                {
                    _logger.Warning("Can't convert a insert's associated chunk to the Word.ContentControl.");
                    continue;
                }
                switch (insert.Kind)
                {
                    case InsertKind.CheckMark:
                        control.Checked = insert.ReplacedCheckmark.Value;
                        break;
                    default: // InsertKind.PlainText
                        control.Range.Text = insert.ReplacedText;
                        break;
                }
                // Убирает пустые поля с текстом "Место для ввода текста."
                control.Delete();
            }
        }

        public string Export(IDocument document)
        {
            var wordDocument = (WordDocument)document;
            var output = Path.Combine(TemporaryOutputDirectory, "Output.pdf");
            wordDocument.Body.ExportAsFixedFormat(output, Word.WdExportFormat.wdExportFormatPDF);
            return output;
        }

        #region IDisposable impl

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ;
                }
#pragma warning disable CA1416 // Проверка совместимости платформы
                if (app != null)
                {
                    object saveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
                    object originalFormat = Missing.Value;
                    object routeDocument = Missing.Value;
                    app.Quit(ref saveChanges, ref originalFormat, ref routeDocument);
                    // Освобождение COM объектов
                    // Может также пригодиться Marshal.ReleaseComObject(app)
#if DEBUG
                    if (Marshal.FinalReleaseComObject(app) != 0
                        && Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
#else
                    Marshal.FinalReleaseComObject(app);
#endif
                }
#pragma warning restore CA1416 // Проверка совместимости платформы
                app = null;
                disposedValue = true;
            }
        }

        ~WordService()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
