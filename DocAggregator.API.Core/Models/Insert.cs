namespace DocAggregator.API.Core.Models
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
        /// <summary>
        /// Таблица или иная встроеная форма.
        /// </summary>
        MultiField,
    }

    /// <summary>
    /// Вставка - элемент документа, заполняемый информацией из базы данных.
    /// </summary>
    /// <remarks>
    /// Может ссылаться на несколько полей.
    /// </remarks>
    public class Insert
    {
        // TODO: DELETE logger?
        /// <remarks>
        /// Use with ?. op only!
        /// </remarks>
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
                _logger?.Debug($"{GetType()}.{nameof(ReplacedCheckmark)} returns \"{replacedCheckmark}\".");
                return replacedCheckmark;
            }
            set
            {
                // TODO: DELETE
                _logger?.Debug($"{GetType()}.{nameof(ReplacedCheckmark)} has changed from \"{replacedCheckmark}\" to \"{value}\".");
                replacedCheckmark = value;
            }
        }

        /// <summary>
        /// Исходная строка формата с полем(ями).
        /// </summary>
        public string OriginalMask { get; set; }

        /// <summary>
        /// Исходная строка тегов.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Обпределяет требование значения для поля.
        /// </summary>
        public bool Required => Tag.ToLower().Contains("required");

        /// <summary>
        /// Специфичный объект элемента документа, асоциированный с этой вставкой.
        /// </summary>
        public object AssociatedChunk { get; set; }

        /// <summary>
        /// Создаёт копию данной вставки.
        /// </summary>
        /// <param name="source">Исходная вставка</param>
        public Insert(Insert source, ILogger logger = null)
        {
            _logger = logger;
            OriginalMask = source.OriginalMask;
            Tag = source.Tag;
            Kind = source.Kind;
            // TODO: DELETE
            _logger?.Debug($"{GetType()} created with arguments (\"{source.OriginalMask}\", \"{source.Tag}\", {source.Kind}).");
        }

        /// <summary>
        /// Создаёт вставку с заданной строкой формата и типом.
        /// </summary>
        /// <param name="mask">Строка формата, содержащяя поля заявки.</param>
        /// <param name="tag">Строка валидации.</param>
        /// <param name="kind">Тип элемента документа.</param>
        public Insert(string mask, string tag, InsertKind kind = InsertKind.PlainText, ILogger logger = null)
        {
            _logger = logger;
            OriginalMask = mask;
            Tag = tag;
            Kind = kind;
            // TODO: DELETE
            _logger?.Debug($"{GetType()} created with arguments (\"{mask}\", \"{tag}\", {kind}).");
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
