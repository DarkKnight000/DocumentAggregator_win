using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Infrastructure.CustomEditor
{
    public class Document : IDocument
    {
        public string ResultPath { get; set; }
    }

    public class EditorService : IEditorService, IDisposable
    {
        Process _serverConverterProc;

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

        public EditorService()
        {
            string libraOfficeFolder = @"C:\Program Files\LibreOffice\program\";
            string pythonExecutable = Path.Combine(libraOfficeFolder, @"python-core-3.8.10\bin\python.exe");
            // Check Python
            if (!File.Exists(pythonExecutable))
            {
                Debugger.Break();
            }
            string unoserviceScriptsFolder = Path.GetFullPath(@"..\unoserver\src\unoserver\");
            // Check scripts
            if (!Directory.Exists(unoserviceScriptsFolder))
            {
                Debugger.Break();
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                WorkingDirectory = unoserviceScriptsFolder,
                FileName = pythonExecutable,
                Arguments = $"server.py --executable \"{libraOfficeFolder}\"",
            };
            processStartInfo.Environment["PATH"] = processStartInfo.Environment["PATH"] + $";{libraOfficeFolder}";
            _serverConverterProc = Process.Start(processStartInfo);
        }

        public IDocument OpenTemplate(string path)
        {
            return new Document() { ResultPath = Path.Combine(TemplatesDirectory, path) };
        }

        public IEnumerable<Insert> GetInserts(IDocument document)
        {
            yield break;
        }

        public void SetInserts(IDocument document, IEnumerable<Insert> inserts)
        {
            return;
        }

        public string Export(IDocument document)
        {
            string libraOfficeFolder = @"C:\Program Files\LibreOffice\program\";
            string pythonExecutable = Path.Combine(libraOfficeFolder, @"python-core-3.8.10\bin\python.exe");
            string unoserviceScriptsFolder = @"..\unoserver\src\unoserver\";
            string outputFile = Path.Combine(TemporaryOutputDirectory, "Output.pdf");
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                WorkingDirectory = unoserviceScriptsFolder,
                FileName = pythonExecutable,
                // TODO: use stdin & stdout (with --convert-to pdf)
                Arguments = $"converter.py \"{((Document)document).ResultPath}\" \"{outputFile}\"",
            };
            processStartInfo.Environment["PATH"] = processStartInfo.Environment["PATH"] + $";{libraOfficeFolder}";
            Process convertingProcess = Process.Start(processStartInfo);
            convertingProcess.WaitForExit();
            if (convertingProcess.ExitCode != 0)
            {
                Debugger.Break();
            }
            return outputFile;
        }

        #region IDisposable impl

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _serverConverterProc.Close();
                    _serverConverterProc.WaitForExit();
                    if (_serverConverterProc.ExitCode != 0)
                    {
                        Debugger.Break();
                    }
                }

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
