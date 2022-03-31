using System.Collections.Generic;
using System.IO;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Описывает абстрактный документ.
    /// </summary>
    public interface IDocument { }

    /// <summary>
    /// Описывает редактор документа.
    /// </summary>
    public interface IEditorService
    {
        /// <summary>
        /// Получает или задаёт путь к физическому метоположению заявок.
        /// </summary>
        public abstract string TemplatesDirectory { get; set; }

        /// <summary>
        /// Открывает документ на основе заданного шаблона.
        /// </summary>
        /// <param name="resultFile">Путь к шаблону.</param>
        /// <returns>Документ, представляющий открытый шаблон.</returns>
        IDocument OpenTemplate(string resultFile);

        /// <summary>
        /// Получает список вставок из документа.
        /// </summary>
        /// <param name="document">Экземпляр документа.</param>
        /// <returns>Перечисление вставок.</returns>
        IEnumerable<Insert> GetInserts(IDocument document);

        /// <summary>
        /// Устанавликает список вставок в документ.
        /// </summary>
        /// <param name="document">Экземпляр документа.</param>
        /// <param name="inserts">Перечисление вставок.</param>
        void SetInserts(IDocument document, IEnumerable<Insert> inserts);

        /// <summary>
        /// Производит сохранение документа в формате PDF.
        /// </summary>
        /// <param name="document">Экземпляр документа.</param>
        /// <returns>Поток со сконвертированным файлом.</returns>
        Stream Export(IDocument document);
    }
}
