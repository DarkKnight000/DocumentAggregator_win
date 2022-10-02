using DocAggregator.API.Core;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Linq;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class ModelBind
    {
        private ILogger _logger;
        private StringDictionary _dataBindings;

        public StringDictionary DataBindings => _dataBindings;

        public ModelBind(IOptionsFactory optionsFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLoggerFor<ModelBind>();
            var db = optionsFactory.GetOptionsOf<RepositoryConfigOptions>();
            _dataBindings = new StringDictionary();
            string[] files = null;
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
