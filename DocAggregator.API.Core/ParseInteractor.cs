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
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID) == insertionFormat
                ).SingleOrDefault()?.ToBoolean() ?? false;
        }

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
            return claim.AccessRightFields.Where(
                    arf => arf.NumeralID.ToString() == insertionFormat
                ).SingleOrDefault()?.Status.Equals(accessRight) ?? false;
        }

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
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID ?? "") == insertionFormat
                ).SingleOrDefault()?.Value ?? "";
        }

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
