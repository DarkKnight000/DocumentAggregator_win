using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.IO;
using Word = Microsoft.Office.Interop.Word;

namespace DocAggregator.API.Infrastructure.OfficeInterop
{
    public class WordService : IEditorService<WordDocument>, IDisposable
    {
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

        public WordService()
        {
            // Очистка кучи для предупреждения COM+ ошибки HRESULT: 0x80080005 (не работает)
            GC.Collect();
            app = new Word.Application();
        }

        public WordDocument OpenTemplate(string path)
        {
            WordDocument result = new WordDocument();
            string file = Path.Combine(TemplatesDirectory, path);
            if (!File.Exists(file))
            {
                // TODO: log this
                return null;
            }
            object template = file;
            object newTemp = false;
            object docType = Word.WdNewDocumentType.wdNewBlankDocument;
            object vis = false;
            Word.Document doc = app.Documents.Add(Template: ref template, NewTemplate: ref newTemp, DocumentType: ref docType, Visible: ref vis);
            result.Body = doc;
            return result;
        }

        public IEnumerable<Insert> GetInserts(WordDocument document)
        {
            if (document.Body == null)
            {
                // TODO: log this
                yield break;
            }
            var range = document.Body.Range(document.Body.Content.Start, document.Body.Content.End);
            foreach (Word.ContentControl control in range.ContentControls)
            {
                Console.WriteLine($"Register content control {control.Type} with a Title \"{control.Title}\".");
                if (control.Title == null)
                {
                    // TODO: log this
                    Console.WriteLine("The Title is null! Skipped.");
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
                        // TODO: log this
                        Console.WriteLine("Unknown control type!");
                        break;
                }
                yield return new Insert(control.Title, kind) { AssociatedChunk = control };
            }
        }

        public void SetInserts(WordDocument document, IEnumerable<Insert> inserts)
        {
            foreach (Insert insert in inserts)
            {
                Console.WriteLine($"Putting \"{insert.ReplacedText ?? insert.ReplacedCheckmark.ToString()}\" in the control with tag \"{insert.OriginalMask}\".");
                Word.ContentControl control = insert.AssociatedChunk as Word.ContentControl;
                if (control == null)
                {
                    // TODO: log this
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

        public string Export(WordDocument document)
        {
            var output = Path.Combine(TemporaryOutputDirectory, "Output.pdf");
            document.Body.ExportAsFixedFormat(output, Word.WdExportFormat.wdExportFormatPDF);
            return output;
        }

        #region IDisposable impl

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    object save = false;
                    app.Quit(ref save);
                }
#pragma warning disable CA1416 // Проверка совместимости платформы
                // Освобождение COM объектов
                // Может также пригодиться Marshal.ReleaseComObject(app)
#if DEBUG
                if (System.Runtime.InteropServices.Marshal.FinalReleaseComObject(app) != 0
                    && System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
#else
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(app);
#endif
#pragma warning restore CA1416 // Проверка совместимости платформы
                disposedValue = true;
            }
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
