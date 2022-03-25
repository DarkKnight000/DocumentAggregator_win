using DocAggregator.API.Core;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;

namespace DocAggregator.API.Infrastructure.OfficeInterop
{
    [Obsolete("Use the OpenXMLProcessing.WordMLDOcument with the OpenXMLProcessing.EditorService class instead.", true)]
    public class WordDocument : IDocument, IDisposable
    {
        private bool disposedValue;

        public Word.Document Body { get; set; }

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
                if (Body != null)
                {
                    object saveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
                    object originalFormat = Missing.Value;
                    object routeDocument = Missing.Value;
                    Body.Close(ref saveChanges, ref originalFormat, ref routeDocument);
                    Marshal.FinalReleaseComObject(Body);
                }
#pragma warning restore CA1416 // Проверка совместимости платформы
                Body = null;
                disposedValue = true;
            }
        }

        ~WordDocument()
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
