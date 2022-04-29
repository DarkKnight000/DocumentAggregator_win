namespace DocAggregator.API.Core.Models
{
    /// <summary>
    /// Поле права доступа.
    /// </summary>
    public class AccessRightField
    {
        /// <summary>
        /// Числовой идентивикатор права доступа.
        /// </summary>
        public int NumeralID { get; init; }

        /// <summary>
        /// Имя роли.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Статус права доступа.
        /// </summary>
        public AccessRightStatus Status { get; init; }

        /// <summary>
        /// Сокращённая проверка на запрос доступа.
        /// </summary>
        public bool IsAllowed => Status.HasFlag(AccessRightStatus.Allowed);

        /// <summary>
        /// Сокращённая проверка на отзыв доступа.
        /// </summary>
        public bool IsDenied => Status.HasFlag(AccessRightStatus.Denied);
    }
}