using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class ParseInsertInteractor
    {
        IMixedFieldRepository _fieldRepo;

        public ParseInsertInteractor(IMixedFieldRepository fieldRepository)
        {
            _fieldRepo = fieldRepository;
        }

        public InsertResponse Handle(InsertRequest request)
        {
            var response = new InsertResponse();
            response.Inserts = request.Inserts;
            foreach (Insert insert in request.Inserts)
            {
                if (insert.Kind == InsertKind.CheckMark)
                {
                    insert.ReplacedCheckmark = ParseBoolField(insert.OriginalMask);
                }
                else
                {
                    insert.ReplacedText = ParseField(insert.OriginalMask);
                }
            }
            return response;
        }

        bool ParseBoolField(string insertionFormat)
        {
            if (insertionFormat.StartsWith('!'))
            {
                insertionFormat = insertionFormat.Substring(1);
                return  !bool.Parse(_fieldRepo.GetFieldByNameOrId(insertionFormat));
            }
            else
            {
                return  bool.Parse(_fieldRepo.GetFieldByNameOrId(insertionFormat));
            }
        }

        string ParseField(string insertionFormat)
        {
            string recursiveResult;
            if (TryParseDelimetedFields(insertionFormat, ',', ", ", out recursiveResult))
            {
                return recursiveResult;
            }
            if (TryParseDelimetedFields(insertionFormat, '/', " / ", out recursiveResult))
            {
                return recursiveResult;
            }
            return _fieldRepo.GetFieldByNameOrId(insertionFormat) ?? "";
        }

        bool TryParseDelimetedFields(string insertionFormat, char delimiter, string connector, out string result)
        {
            if (insertionFormat.Contains(delimiter))
            {
                string[] parts = insertionFormat.Split(delimiter, 2);
                string left, right;
                left = ParseField(parts[0]);
                right = ParseField(parts[1]);
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
