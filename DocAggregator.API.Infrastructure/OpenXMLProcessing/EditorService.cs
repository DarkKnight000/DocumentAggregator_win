using DocAggregator.API.Core;
using DocAggregator.API.Core.Wml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace DocAggregator.API.Infrastructure.OpenXMLProcessing
{
    /// <summary>
    /// Реализация редактора документа.
    /// </summary>
    /// <remarks>
    /// Использует <see cref="WordprocessingMLEditor"/> для работы на уровне элементов XML.
    /// </remarks>
    public class EditorService : IEditorService, IDisposable
    {
        ILogger _logger;
        WordprocessingMLEditor _wmlEditor;
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

        [Obsolete]
        public string TemporaryOutputDirectory
        {
            get => _temporaryOutputDirectory;
            set
            {
                _temporaryOutputDirectory = Path.GetFullPath(value);
            }
        }
        private string _temporaryOutputDirectory;

        /// <summary>
        /// Получает или задаёт путь к пакету программ LibreOffice.
        /// </summary>
        public string LibreOfficeFolder
        {
            get => _libreOfficeFolder;
            set
            {
                _libreOfficeFolder = Path.GetFullPath(value);
                _libreOfficeExecutable = Path.Combine(_libreOfficeFolder, "soffice");
            }
        }
        public string LibreOfficeExecutable
        {
            get => _libreOfficeExecutable;
        }
        private string _libreOfficeFolder;// = @"C:\Program Files\LibreOffice\program";
        private string _libreOfficeExecutable;

        /// <summary>
        /// Получает или задаёт путь к скриптам unoclient и unoserver.
        /// </summary>
        public string Scripts
        {
            get => _scripts;
            set
            {
                _scripts = Path.GetFullPath(value);
            }
        }
        private string _scripts;// = @"D:\Users\akkostin\source\repos\DocumentAggregator\unoserver\src\unoserver";

        private bool initializedValue;
        private bool disposedValue;

        public EditorService(IOptionsFactory options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<IEditorService>();
            _wmlEditor = new WordprocessingMLEditor(_logger);

            var editor = options.GetOptionsOf<EditorConfigOptions>();
            TemplatesDirectory = editor.TemplatesDir;
            //TemporaryOutputDirectory = editor.OutputDir;
            LibreOfficeFolder = editor.LibreOffice;
            Scripts = editor.Scripts;
        }

        public void EnsureInitialize()
        {
            if (!initializedValue)
            {
                Initialize();
                initializedValue = true;
            }
        }

        private void Initialize()
        {
            // Check Python
            if (!File.Exists(Path.Combine(LibreOfficeFolder, "python.exe")))
            {
                Debugger.Break();
            }
            // Check scripts
            if (!Directory.Exists(Scripts))
            {
                Debugger.Break();
            }
            // cmd> set PATH=%PATH%;C:\Program Files\LibreOffice\program
            string envPATH = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) ?? string.Empty;
            if (!string.IsNullOrEmpty(envPATH) && !envPATH.EndsWith(";"))
                envPATH = envPATH + ';';
            envPATH = envPATH + LibreOfficeFolder;
            Environment.SetEnvironmentVariable("PATH", envPATH, EnvironmentVariableTarget.Process);
            // If server is not launched yet.
            if (Process.GetProcessesByName("soffice").Length == 0)
            {
                // Did not found a LibreOffice process.
                // Starting a server.
                ProcessStartInfo processServerInfo = new ProcessStartInfo()
                {
                    // cmd> cd "D:\Users\akkostin\source\repos\DocumentAggregator\unoserver\src\unoserver"
                    WorkingDirectory = Scripts,
                    FileName = "python",
                    Arguments = $"server.py --executable \"{LibreOfficeExecutable}\"",
                };
                // cmd> python server.py --executable "C:\Program Files\LibreOffice\program\soffice"
                _serverConverterProc = new Process();
                _serverConverterProc.StartInfo = processServerInfo;
                // _serverConverterProc.Exited += (s, a) => _logger.Debug("[serv] Exited");
                _serverConverterProc.Start();
            }
        }

        public IDocument OpenTemplate(string path)
        {
            EnsureInitialize();
            //string tempFile = Path.Combine(TemporaryOutputDirectory, "TempCopy.docx");
            // TODO: Copy into memory
            //File.Copy(Path.Combine(TemplatesDirectory, path), tempFile, true);
            MemoryStream tempStream = new MemoryStream();
            File.OpenRead(Path.Combine(TemplatesDirectory, path)).CopyTo(tempStream);
            return new WordMLDocument(tempStream);
        }

        public IEnumerable<Insert> GetInserts(IDocument document)
        {
            EnsureInitialize();
            // return (document as WordMLDocument).GetInserts();
            return _wmlEditor.FindInserts((document as WordMLDocument).MainPart);
        }

        public void SetInserts(IDocument document, IEnumerable<Insert> inserts)
        {
            EnsureInitialize();
            // (document as WordMLDocument).SetInserts(inserts);
            _wmlEditor.SetInserts(System.Linq.Enumerable.ToArray(inserts));
        }

        public Stream Export(IDocument document)
        //public string Export(IDocument document)
        {
            EnsureInitialize();
            //string outputFile = Path.Combine(TemporaryOutputDirectory, "Output.pdf");
            WordMLDocument documentContainer = document as WordMLDocument;
            //string inputFile = documentContainer.ResultPath;
            var wordDocument = documentContainer.Content;
            _logger.Trace("Save an edited part back into a stream.");
            using (var xw = XmlWriter.Create(wordDocument.MainDocumentPart.GetStream(FileMode.Create, FileAccess.Write)))
            {
                documentContainer.MainPart.Save(xw);
            }
            wordDocument.Save();
            //wordDocument.Close();
            // cmd> set PATH=%PATH%;C:\Program Files\LibreOffice\program
            string envPATH = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) ?? string.Empty;
            if (!string.IsNullOrEmpty(envPATH) && !envPATH.EndsWith(";"))
                envPATH = envPATH + ';';
            envPATH = envPATH + LibreOfficeFolder;
            Environment.SetEnvironmentVariable("PATH", envPATH, EnvironmentVariableTarget.Process);
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                // cmd> cd "D:\Users\akkostin\source\repos\DocumentAggregator\unoserver\src\unoserver"
                WorkingDirectory = Scripts,
                FileName = "python",
                // TODO: Read and Write using stdin & stdout (with --convert-to pdf)
                //Arguments = $"converter.py \"{inputFile}\" \"{outputFile}\"",
                //Arguments = $"converter.py --convert-to pdf \"{inputFile}\" -",
                Arguments = $"converter.py --convert-to pdf - -",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            Process convertingProcess = new Process() { StartInfo = processStartInfo };
            convertingProcess.ErrorDataReceived += (sender, args) =>
            {
                _logger.Trace("Converter standard error output:\r\n{0}", args.Data);
            };
            _logger.Trace("Start a converter.");
            convertingProcess.Start();
            convertingProcess.BeginErrorReadLine();
            documentContainer.ResultStream.Seek(0, SeekOrigin.Begin);
            _logger.Debug("Copying a result stream to the standard input of the converter.");
            documentContainer.ResultStream.CopyTo(convertingProcess.StandardInput.BaseStream);
            _logger.Debug("Flushing all data in input stream and closing it.");
            convertingProcess.StandardInput.Flush();
            convertingProcess.StandardInput.Close();
            //_logger.Debug("Instead writing to {0}, use a memory stream.", outputFile);
            _logger.Debug("Reading converter output.");
            var outputStream = new MemoryStream();
            convertingProcess.StandardOutput.BaseStream.CopyTo(outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            convertingProcess.WaitForExit();
            if (convertingProcess.ExitCode != 0)
            {
                _logger.Error("Converter exited with an exit code {0}.", convertingProcess.ExitCode);
                Debugger.Break();
            }
            wordDocument.Close();
            return outputStream;
            //return outputFile;
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
