using DocAggregator.API.Core.Models;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Исключение, возникаемое при попытке установить обязательному полю пустое значение.
    /// </summary>
    public class SolvableValidationException : SolvableException
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SolvableValidationException"/>.
        /// </summary>
        public SolvableValidationException() : base() { }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SolvableValidationException"/>
        /// с сообщением о содержимом вставки.
        /// </summary>
        /// <param name="source">Объект, не прошедший валидацию.</param>
        public SolvableValidationException(Insert source)
            : base($"вставка типа {source.Kind} с запросом {{{source.OriginalMask}}} осталась без требуемого значения",
                  "проверьте модель документа, сопоствьте значение из запроса вставки, добавьте поле в запросах модели",
                  "проверьте результат выполнения запроса модели, наличие соответствующих данных в БД и ограничения фильтров",
                  "снимите требование валидации") { }
    }
}
