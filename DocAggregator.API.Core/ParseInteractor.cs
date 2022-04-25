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
        public int ClaimID { get; set; }

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
                    insert.ReplacedCheckmark = ParseBoolField(request.ClaimID, insert.OriginalMask);
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = ParseField(request.ClaimID, insert.OriginalMask);
                    break;
            }
        }

        bool ParseBoolField(int claimID, string insertionFormat)
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
                        break;
                    case "c":
                        accessRight = AccessRightStatus.Changed;
                        break;
                    case "d":
                        accessRight = AccessRightStatus.Denied;
                        break;
                }
                return _fieldRepo.GetAccessRightByIdAndStatus(claimID, insertionFormat, accessRight);
            }
            string fieldVal;
            if (insertionFormat.StartsWith('!'))
            {
                return !ParseBoolField(claimID, insertionFormat.Substring(1));
            }
            else
            {
                fieldVal = _fieldRepo.GetFieldByNameOrId(claimID, insertionFormat);
                return fieldVal != null ? bool.Parse(fieldVal) : false;
            }
        }

        string ParseField(int claimID, string insertionFormat)
        {
            string recursiveResult;
            if (TryParseDelimetedFields(claimID, insertionFormat, ',', ", ", out recursiveResult))
            {
                return recursiveResult;
            }
            if (TryParseDelimetedFields(claimID, insertionFormat, '/', " / ", out recursiveResult))
            {
                return recursiveResult;
            }
            return _fieldRepo.GetFieldByNameOrId(claimID, insertionFormat) ?? "";
        }

        bool TryParseDelimetedFields(int claimID, string insertionFormat, char delimiter, string connector, out string result)
        {
            if (insertionFormat.Contains(delimiter))
            {
                string[] parts = insertionFormat.Split(delimiter, 2);
                string left, right;
                left = ParseField(claimID, parts[0]);
                right = ParseField(claimID, parts[1]);
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
