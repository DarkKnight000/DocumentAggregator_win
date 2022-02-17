namespace DocAggregator.API.Core
{
    /// <summary>
    /// Представляет репозиторий числовых и именованных полей заявки.
    /// </summary>
    public interface IMixedFieldRepository
    {
        /// <summary>
        /// Получает значение числового или именованного поля по идентификатору заявки.
        /// </summary>
        /// <param name="claimID">Идентификатор заявки.</param>
        /// <param name="fieldName">Идентификатор поля.</param>
        /// <returns>Строковое представление значения поля.</returns>
        public string GetFieldByNameOrId(int claimID, string fieldName);
    }
}
