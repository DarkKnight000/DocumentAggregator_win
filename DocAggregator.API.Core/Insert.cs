using System;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Вариант вставки.
    /// </summary>
    public enum InsertKind
    {
        /// <summary>
        /// Неизвестный тип.
        /// </summary>
        Unknown,

        /// <summary>
        /// Обыкновенный текст.
        /// </summary>
        PlainText,

        /// <summary>
        /// Флажок.
        /// </summary>
        CheckMark,
    }

    /// <summary>
    /// Вставка - элемент документа, заполняемый информацией из базы данных.
    /// </summary>
    public class Insert
    {
        // TODO: DELETE logger?
        ILogger _logger;
        // TODO: DELETE Проблема была с yield return, который возвращал объект с другим значением.
        private bool? replacedCheckmark;

        /// <summary>
        /// Вид элемента.
        /// </summary>
        public InsertKind Kind { get; private set; }

        /// <summary>
        /// Обработанный текст.
        /// </summary>
        public string ReplacedText { get; set; }

        /// <summary>
        /// Обработанное значение (для элементов типа флажок).
        /// </summary>
        public bool? ReplacedCheckmark
        {
            get
            {
                // TODO: DELETE
                _logger.Debug($"{GetType()}.{nameof(ReplacedCheckmark)} returns \"{replacedCheckmark}\".");
                return replacedCheckmark;
            }
            set
            {
                // TODO: DELETE
                _logger.Debug($"{GetType()}.{nameof(ReplacedCheckmark)} has changed from \"{replacedCheckmark}\" to \"{value}\".");
                replacedCheckmark = value;
            }
        }

        /// <summary>
        /// Исходная строка формата с полем(ями).
        /// </summary>
        public string OriginalMask { get; set; }

        /// <summary>
        /// Специфичный объект элемента документа, асоциированный с этой вставкой.
        /// </summary>
        public object AssociatedChunk { get; set; }

        /// <summary>
        /// Создаёт вставку с заданной строкой формата и типом.
        /// </summary>
        /// <param name="mask">Строка формата, содержащяя поля заявки.</param>
        /// <param name="kind">Тип элемента документа.</param>
        public Insert(string mask, InsertKind kind = InsertKind.PlainText, ILogger logger = null)
        {
            _logger = logger;
            OriginalMask = mask;
            Kind = kind;
            // TODO: DELETE
            _logger.Debug($"{GetType()} created with arguments (\"{mask}\", {kind}).");
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Insert other = obj as Insert;
            if (Kind != other.Kind)
            {
                return false;
            }
            return OriginalMask.Equals(other.OriginalMask);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Kind.GetHashCode() ^ OriginalMask.GetHashCode();
        }
    }
}
