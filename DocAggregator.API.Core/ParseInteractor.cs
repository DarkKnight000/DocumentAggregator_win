using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class ParseRequest
    {
        public int ClaimID { get; set; }
        public Insert Insertion { get; set; }
    }
    public class ParseResponse : InteractorResponseBase { }

    public class ParseInteractor : InteractorBase<ParseResponse, ParseRequest>
    {
        // TODO: Убрать лишние дополнительные claim_id поля (возможно переместить внутрь Insert)
        IMixedFieldRepository _fieldRepo;

        public ParseInteractor(IMixedFieldRepository fieldRepository)
        {
            _fieldRepo = fieldRepository;
        }

        protected override void Handle()
        {
            var insert = Request.Insertion;
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    insert.ReplacedCheckmark = ParseBoolField(Request.ClaimID, insert.OriginalMask);
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = ParseField(Request.ClaimID, insert.OriginalMask);
                    break;
            }
        }

        bool ParseBoolField(int claimID, string insertionFormat)
        {
            if (insertionFormat.StartsWith('!'))
            {
                insertionFormat = insertionFormat.Substring(1);
                return !bool.Parse(_fieldRepo.GetFieldByNameOrId(claimID, insertionFormat));
            }
            else
            {
                return bool.Parse(_fieldRepo.GetFieldByNameOrId(claimID, insertionFormat));
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
