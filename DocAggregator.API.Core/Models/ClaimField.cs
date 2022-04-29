namespace DocAggregator.API.Core.Models
{
    /// <summary>
    /// Поле заявки.
    /// </summary>
    public class ClaimField
    {
        /// <summary>
        /// Числовой идентификатор заявки.
        /// </summary>
        public int? NumeralID { get; init; }
        
        /// <summary>
        /// Именной идентификатор, если имеется, иначе равен вызову <see cref="NumeralID"/>.ToString() .
        /// </summary>
        public string VerbousID
        {
            get => _verbousID ?? NumeralID?.ToString();
            init => _verbousID = value;
        }
        private readonly string _verbousID;

        /// <summary>
        /// Категория поля заявки.
        /// </summary>
        public string Category { get; init; }

        /// <summary>
        /// Имя атрибута (поля) заявки.
        /// </summary>
        public string Attribute { get; init; }

        /// <summary>
        /// Значение поля.
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// Конвертирует значение поля в логическое значение.
        /// </summary>
        /// <returns>true, если установлено значение равное <see cref="bool.TrueString"/>, иначе false.</returns>
        public bool ToBoolean() => bool.TryParse(Value, out bool result) & result;
    }
}
