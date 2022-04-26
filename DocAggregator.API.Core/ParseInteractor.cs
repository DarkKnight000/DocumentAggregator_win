using DocAggregator.API.Core.Models;

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
        // TODO: Убрать лишние дополнительные claim_id поля (возможно переместить внутрь Insert)
        IClaimFieldRepository _fieldRepo;

        /// <summary>
        /// Создаёт обработчик на основе репозитория полей заявки.
        /// </summary>
        /// <param name="fieldRepository">Репозиторий полей заявки.</param>
        public ParseInteractor(IClaimFieldRepository fieldRepository, ILoggerFactory loggerFactory)
            : base(loggerFactory.GetLoggerFor<ParseInteractor>())
        {
            _fieldRepo = fieldRepository;
        }

        protected override void Handle(ParseResponse response, ParseRequest request)
        {
            var insert = request.Insertion;
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    insert.ReplacedCheckmark = ParseBoolField(request.Claim, insert.OriginalMask);
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = ParseField(request.Claim, insert.OriginalMask);
                    break;
            }
        }

        bool ParseBoolField(Claim claim, string insertionFormat)
        {
            if (insertionFormat.StartsWith('*'))
            {
                string state = insertionFormat.Substring(insertionFormat.Length - 1);
                insertionFormat = insertionFormat.Substring(1, insertionFormat.Length - 2);
                AccessRightStatus accessRight = AccessRightStatus.NotMentioned;
                switch (state)
                {
                    case "a":
                        accessRight = AccessRightStatus.Allowed;
                        return _fieldRepo.GetAccessRightByIdAndStatus(claim, insertionFormat, accessRight).IsAllowed;
                    case "c":
                        accessRight = AccessRightStatus.Changed;
                        return _fieldRepo.GetAccessRightByIdAndStatus(claim, insertionFormat, accessRight).Status == AccessRightStatus.Changed;
                    case "d":
                        accessRight = AccessRightStatus.Denied;
                        return _fieldRepo.GetAccessRightByIdAndStatus(claim, insertionFormat, accessRight).IsDenied;
                }
            }
            string fieldVal;
            if (insertionFormat.StartsWith('!'))
            {
                return !ParseBoolField(claim, insertionFormat.Substring(1));
            }
            else
            {
                fieldVal = _fieldRepo.GetFieldByNameOrId(claim, insertionFormat).Value;
                return fieldVal != null ? bool.Parse(fieldVal) : false;
            }
        }

        string ParseField(Claim claim, string insertionFormat)
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
            return _fieldRepo.GetFieldByNameOrId(claim, insertionFormat)?.Value ?? "";
        }

        bool TryParseDelimetedFields(Claim claim, string insertionFormat, char delimiter, string connector, out string result)
        {
            if (insertionFormat.Contains(delimiter))
            {
                string[] parts = insertionFormat.Split(delimiter, 2);
                string left, right;
                left = ParseField(claim, parts[0]);
                right = ParseField(claim, parts[1]);
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
