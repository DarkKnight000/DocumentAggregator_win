using System.Collections.Generic;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Описывает абстрактный документ.
    /// </summary>
    public interface IDocument { }

    /// <summary>
    /// Описывает редактор документа.
    /// </summary>
    /// <typeparam name="TDocument">Тип для содержания внутреннего состояния документа.</typeparam>
    public interface IEditorService<TDocument> where TDocument : IDocument
    {
        /// <summary>
        /// Открывает документ на основе заданного шаблона.
        /// </summary>
        /// <param name="path">Путь к шаблону.</param>
        /// <returns>Документ, представляющий открытый шаблон.</returns>
        TDocument OpenTemplate(string path);

        /// <summary>
        /// Получает список вставок из документа.
        /// </summary>
        /// <param name="document">Экземпляр документа.</param>
        /// <returns>Перечисление вставок.</returns>
        IEnumerable<Insert> GetInserts(TDocument document);

        /// <summary>
        /// Устанавликает список вставок в документ.
        /// </summary>
        /// <param name="document">Экземпляр документа.</param>
        /// <param name="inserts">Перечисление вставок.</param>
        void SetInserts(TDocument document, IEnumerable<Insert> inserts);

        /// <summary>
        /// Производит сохранение документа в формате PDF.
        /// </summary>
        /// <param name="document">Экземпляр документа.</param>
        /// <returns>Путь к сконвертированному файлу.</returns>
        string Export(TDocument document);
    }
}
