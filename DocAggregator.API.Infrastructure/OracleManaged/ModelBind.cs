using DocAggregator.API.Core;
using System;
using System.Collections.Specialized;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Xml.Linq;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class ModelBind
    {
        private ILogger _logger;
        private StringDictionary _dataBindings;
        private FileSystemWatcher _templateBindingsWatcher;

        public StringDictionary DataBindings => _dataBindings;

        public ModelBind(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ModelBind>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            _dataBindings = new StringDictionary();
            string[] files = null;
            _templateBindingsWatcher = new FileSystemWatcher(db.TemplateBindings, "*.xml");
            _templateBindingsWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
            _templateBindingsWatcher.Created += (s, a) =>
            {
                string type = Path.GetFileNameWithoutExtension(a.FullPath).ToLower();
                _dataBindings.Add(type, a.FullPath);
                _logger.Trace("Template binding for {0} has created.", type);
            };
            _templateBindingsWatcher.Renamed += (s, a) =>
            {
                string oldType = Path.GetFileNameWithoutExtension(a.OldFullPath).ToLower(),
                       type = Path.GetFileNameWithoutExtension(a.FullPath).ToLower();
                _dataBindings.Remove(oldType);
                _dataBindings.Add(type, a.FullPath);
                _logger.Trace("Template binding has updated (renamed) from {0} to {1}.", oldType, type);
            };
            _templateBindingsWatcher.Deleted += (s, a) =>
            {
                string type = Path.GetFileNameWithoutExtension(a.FullPath).ToLower();
                _dataBindings.Remove(type);
                _logger.Trace("Template binding for {0} has been deleted.", type);
            };
            _templateBindingsWatcher.Error += (s, a) =>
            {
                _logger.Critical(a.GetException(), "The system file watcher of the templates directory has been stoped.");
            };
            try
            {
                files = Directory.GetFiles(db.TemplateBindings, "*.xml");
            }
            catch (Exception ex)
            {
                RepositoryExceptionHelper.ThrowConfigurationModelFolderFailure(ex);
            }
            foreach (var filePath in files)
            {
                _dataBindings.Add(Path.GetFileNameWithoutExtension(filePath).ToLower(), filePath);
            }
            _templateBindingsWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Получает модель по заданному типу документа.
        /// </summary>
        /// <remarks>
        /// Выбрасывает исключение в случае неуспеха.
        /// </remarks>
        /// <param name="documentType">Тип документа.</param>
        /// <returns>XML-представление модели.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="SolvableException"/>
        public XDocument GetBind(string documentType)
        {
            var file = _dataBindings[documentType];
            if (file == null)
            {
                RepositoryExceptionHelper.ThrowModelNotFoundFailure(documentType);
            }
            return XDocument.Load(file);
            
        }
    }
}
