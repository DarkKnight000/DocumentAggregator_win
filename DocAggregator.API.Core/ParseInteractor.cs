using DocAggregator.API.Core.Models;
using System.Linq;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Запрос на разбор вставки.
    /// </summary>
    public class ParseRequest
    {
        /// <summary>
        /// Идентификатор заявки.
        /// </summary>
        public Claim Claim { get; set; }

        /// <summary>
        /// Вставка, изменяемая в процессе обработки.
        /// </summary>
        public Insert Insertion { get; set; }
    }

    /// <summary>
    /// Ответ на разбор вставки.
    /// </summary>
    public class ParseResponse : InteractorResponseBase { }

    /// <summary>
    /// Обработчик разбора вставки.
    /// </summary>
    public class ParseInteractor : InteractorBase<ParseResponse, ParseRequest>
    {
        /// <summary>
        /// Создаёт обработчик на основе репозитория полей заявки.
        /// </summary>
        /// <param name="fieldRepository">Репозиторий полей заявки.</param>
        public ParseInteractor(ILoggerFactory loggerFactory)
            : base(loggerFactory.GetLoggerFor<ParseInteractor>()) { }

        protected override void Handle(ParseResponse response, ParseRequest request)
        {
            var insert = request.Insertion;
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    insert.ReplacedCheckmark = ParseBoolField(request.Claim, insert.OriginalMask);
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = ParseTextField(request.Claim, insert.OriginalMask);
                    break;
            }
        }

        /// <summary>
        /// Вызывается, когда в шаблоне ожидается логическое значение поля.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <remarks>
        /// Значение может быть инвертировано ведущим символом '!' в коде поля.
        /// </remarks>
        /// <returns>true, если значение найденного поля равно <see cref="bool.TrueString"/>, иначе false.</returns>
        bool ParseBoolField(Claim claim, string insertionFormat)
        {
            if (insertionFormat.StartsWith('*'))
            {
                return ParseAccessBoolField(claim, insertionFormat);
            }
            if (insertionFormat.StartsWith('!'))
            {
                return !ParseBoolField(claim, insertionFormat.Substring(1));
            }
            return claim.ClaimFields.Where(
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID).Equals(insertionFormat, System.StringComparison.OrdinalIgnoreCase)
                ).SingleOrDefault()?.ToBoolean() ?? false;
        }

        /// <summary>
        /// Вызывается, когда в логическом поле ожидаются данные права доступа.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns>true, если значение найденного поля равно <see cref="bool.TrueString"/>, иначе false.</returns>
        bool ParseAccessBoolField(Claim claim, string insertionFormat)
        {
            string state = insertionFormat.Substring(insertionFormat.Length - 1);
            insertionFormat = insertionFormat.Substring(1, insertionFormat.Length - 2);
            AccessRightStatus accessRight = AccessRightStatus.NotMentioned;
            switch (state)
            {
                case "a":
                    accessRight = AccessRightStatus.Allowed;
                    break;
                case "c":
                    accessRight = AccessRightStatus.Changed;
                    break;
                case "d":
                    accessRight = AccessRightStatus.Denied;
                    break;
            }
            if (insertionFormat == string.Empty)
            {
                return claim.AccessRightFields.Aggregate(AccessRightStatus.NotMentioned,
                        (ars, arf) => ars | arf.Status
                    ).Equals(accessRight);
            }
            else
            {
                return claim.AccessRightFields.Where(
                        arf => arf.NumeralID.ToString() == insertionFormat
                    ).SingleOrDefault()?.Status.Equals(accessRight) ?? false;
            }
        }

        /// <summary>
        /// Вызывается, когда в шаблоне ожидеатся текст.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns>Текстовое значение поля или пустая строка.</returns>
        string ParseTextField(Claim claim, string insertionFormat)
        {
            string recursiveResult;
            if (TryParseDelimetedFields(claim, insertionFormat, ',', ", ", out recursiveResult))
            {
                return recursiveResult;
            }
            if (TryParseDelimetedFields(claim, insertionFormat, '/', " / ", out recursiveResult))
            {
                return recursiveResult;
            }
            return claim.ClaimFields.Where(
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID ?? "").Equals(insertionFormat, System.StringComparison.OrdinalIgnoreCase)
                ).SingleOrDefault()?.Value ?? "";
        }

        /// <summary>
        /// Позволяет форматировать неколько полей заявки в одном поле.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <param name="delimiter">Искомый разделитель полей.</param>
        /// <param name="connector">Соединитель значений полей.</param>
        /// <param name="result">Результат рекурсивного разрешения кода.</param>
        /// <returns>Значение, указывающее на успешность операции.</returns>
        bool TryParseDelimetedFields(Claim claim, string insertionFormat, char delimiter, string connector, out string result)
        {
            if (insertionFormat.Contains(delimiter))
            {
                string[] parts = insertionFormat.Split(delimiter, 2);
                string left, right;
                left = ParseTextField(claim, parts[0]);
                right = ParseTextField(claim, parts[1]);
                if (left == string.Empty || right == string.Empty)
                {
                    result = left + right;
                }
                else
                {
                    result = left + connector + right;
                }
                return true;
            }
            result = null;
            return false;
        }
    }
}
