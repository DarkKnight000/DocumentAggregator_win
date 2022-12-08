using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
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
        UnoConverterLogger _converterLogger;
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
        private string _libreOfficeFolder;
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
        private string _scripts;

#if !DEBUG
        /// <summary>
        /// Получает или задаёт имя пользователя, от имени которого будет запущен сервис конвертера.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Получает или задаёт пароль пользователя, от имени которого будет запущен сервис конвертера.
        /// </summary>
        public string UserPassword { get; set; }
#endif

        private bool initializedValue;
        private bool disposedValue;

        public EditorService(IOptionsFactory options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<IEditorService>();
            _converterLogger = new UnoConverterLogger(loggerFactory.GetLoggerFor<UnoConverterLogger>());
            _wmlEditor = new WordprocessingMLEditor(_logger);

            var editor = options.GetOptionsOf<EditorConfigOptions>();
            TemplatesDirectory = editor.TemplatesDir;
            LibreOfficeFolder = editor.LibreOffice;
            Scripts = editor.Scripts;
#if !DEBUG
            UserName = editor.UserName;
            UserPassword = editor.UserPassword;
#endif
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
            _logger.Trace("Check Python.");
            if (!File.Exists(Path.Combine(LibreOfficeFolder, "python.exe")))
            {
                Debugger.Break();
            }
            _logger.Trace("Check scripts.");
            if (!Directory.Exists(Scripts))
            {
                Debugger.Break();
            }
            // cmd> set PATH=%PATH%;C:\Program Files\LibreOffice\program
            string envPATH = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) ?? string.Empty;
            if (!envPATH.Contains(LibreOfficeFolder))
            {
                if (!string.IsNullOrEmpty(envPATH) && !envPATH.EndsWith(";"))
                    envPATH = envPATH + ';';
                envPATH = envPATH + LibreOfficeFolder;
                _logger.Debug("Setting LibreOffice folder in PATH variable.");
                Environment.SetEnvironmentVariable("PATH", envPATH, EnvironmentVariableTarget.Process);
            }
            _logger.Trace("Check if server is not launched yet.");
            if (Process.GetProcessesByName("soffice").Length == 0)
            {
                _logger.Information("Did not found a LibreOffice process. Starting a server.");
                ProcessStartInfo processServerInfo = new ProcessStartInfo()
                {
                    // cmd> cd "D:\Users\akkostin\source\repos\DocumentAggregator\unoserver\src\unoserver"
                    WorkingDirectory = Scripts,
                    FileName = "python",
                    Arguments = $"server.py --executable \"{LibreOfficeExecutable}\"",
#if !DEBUG
                    UserName = UserName,
                    PasswordInClearText = UserPassword,
#endif
                };
                // cmd> python server.py --executable "C:\Program Files\LibreOffice\program\soffice"
                _serverConverterProc = new Process();
                _serverConverterProc.StartInfo = processServerInfo;
                _serverConverterProc.Start();
            }
        }

        public IDocument OpenTemplate(string path)
        {
            EnsureInitialize();
            return new WordMLDocument(Path.Combine(TemplatesDirectory, path));
        }

        public IEnumerable<Insert> GetInserts(IDocument document)
        {
            EnsureInitialize();
            if ((document as WordMLDocument).Finalized)
            {
                throw new InvalidOperationException("The document was already finalized.");
            }
            return _wmlEditor.FindInserts((document as WordMLDocument).MainPart);
        }

        public void SetInserts(IDocument document, IEnumerable<Insert> inserts)
        {
            EnsureInitialize();
            if ((document as WordMLDocument).Finalized)
            {
                throw new InvalidOperationException("The document was already finalized.");
            }
            _wmlEditor.SetInserts(System.Linq.Enumerable.ToArray(inserts));
        }

        public Stream Finalize(IDocument document)
        {
            EnsureInitialize();
            WordMLDocument documentContainer = document as WordMLDocument;
            if (documentContainer.Finalized)
            {
                throw new InvalidOperationException("The document was already finalized.");
            }
            string inputFile = documentContainer.TemporaryDocumentPath;
            var wordDocument = documentContainer.Content;
            _logger.Trace("Save an edited part back into a stream.");
            using (var ds = wordDocument.MainDocumentPart.GetStream(FileMode.Create, FileAccess.Write))
            using (var xw = XmlWriter.Create(ds))
            {
                documentContainer.MainPart.Save(xw);
            }
            if (DocumentFormat.OpenXml.Packaging.OpenXmlPackage.CanSave)
            {
                wordDocument.Save();
            }
            wordDocument.Close();
            // cmd> set PATH=%PATH%;C:\Program Files\LibreOffice\program
            string envPATH = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) ?? string.Empty;
            if (!envPATH.Contains(LibreOfficeFolder))
            {
                if (!string.IsNullOrEmpty(envPATH) && !envPATH.EndsWith(";"))
                    envPATH = envPATH + ';';
                envPATH = envPATH + LibreOfficeFolder;
                _logger.Debug("Setting LibreOffice folder in PATH variable.");
                Environment.SetEnvironmentVariable("PATH", envPATH, EnvironmentVariableTarget.Process);
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                // cmd> cd "D:\Users\akkostin\source\repos\DocumentAggregator\unoserver\src\unoserver"
                WorkingDirectory = Scripts,
                FileName = "python",
                Arguments = $"converter.py --convert-to pdf \"{inputFile}\" -",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            Process convertingProcess = new Process() { StartInfo = processStartInfo };
            convertingProcess.ErrorDataReceived += (sender, args) =>
            {
                _converterLogger.Log(args.Data);
            };
            _logger.Trace("Start a converter.");
            convertingProcess.Start();
            convertingProcess.BeginErrorReadLine();
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
            // BASE64 decoding hotfix: white PDF pages
            MemoryStream hotFixOutput = new MemoryStream();
            using (BinaryReader reader = new BinaryReader(outputStream))
            using (BinaryWriter writer = new BinaryWriter(hotFixOutput, System.Text.Encoding.UTF8, true))
            {
                var bytes = reader.ReadBytes((int)outputStream.Length);
                var eofSeq = System.Text.Encoding.UTF8.GetBytes("%%EOF");
                var eofMatch = Locate(bytes, eofSeq);
                writer.Write(bytes, 0, eofMatch[0] + eofSeq.Length + 1); // +1 for the '\n'
            }
            hotFixOutput.Seek(0, SeekOrigin.Begin);
            // hotfix end
            File.Delete(documentContainer.TemporaryDocumentPath);
            documentContainer.Finalized = true;
            if (convertingProcess.ExitCode != 0)
            {
                throw new InvalidOperationException($"Converter exited with an exit code {convertingProcess.ExitCode}.");
            }
            return hotFixOutput;
        }

#region Byte array pattern search

        /*
         * Solution from https://stackoverflow.com/questions/283456/byte-array-pattern-search
         * Made by https://stackoverflow.com/users/36702/jb-evain
         */

        public static int[] Locate(byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Array.Empty<int>();

            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Array.Empty<int>() : list.ToArray();
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }

#endregion

#region IDisposable impl

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_serverConverterProc != null)
                    {
                        _serverConverterProc.Close();
                        if (_serverConverterProc.ExitCode != 0)
                        {
                            Debugger.Break();
                        }
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
