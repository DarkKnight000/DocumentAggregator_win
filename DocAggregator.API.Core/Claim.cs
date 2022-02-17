namespace DocAggregator.API.Core
{
    /// <summary>
    /// Представляет объект заявки.
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Идентификатор заявки, согласно базе данных.
        /// </summary>
        public int ID { get; init; }

        /// <summary>
        /// Шаблон соответствующий типу заявки.
        /// </summary>
        public string Template { get; init; }
    }
}
