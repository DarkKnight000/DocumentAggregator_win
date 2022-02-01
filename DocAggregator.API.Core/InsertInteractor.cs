using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class InsertInteractor
    {
        IAttributeRepository _attrRepo;
        IReferenceRepository _refRepo;

        public InsertInteractor(Claim claim, IAttributeRepository attrRepository, IReferenceRepository refRepository)
        {
            _attrRepo = attrRepository;
            _refRepo = refRepository;
        }

        public BooleanInsertion ParseBoolField(string insertionFormat)
        {
            if (insertionFormat.StartsWith('!'))
            {
                insertionFormat = insertionFormat.Substring(1);
                return new BooleanInsertion()
                {
                    Value = !bool.Parse(GetFieldByName(insertionFormat))
                };
            }
            else
            {
                return new BooleanInsertion()
                {
                    Value = bool.Parse(GetFieldByName(insertionFormat))
                };
            }
        }

        public StringInsertion ParseField(string insertionFormat)
        {
            string recursiveResult;
            if (TryParseDelimetedFields(insertionFormat, ',', ", ", out recursiveResult))
            {
                return new StringInsertion() { Value = recursiveResult };
            }
            if (TryParseDelimetedFields(insertionFormat, '/', " / ", out recursiveResult))
            {
                return new StringInsertion() { Value = recursiveResult };
            }
            return GetFieldByName(insertionFormat);
        }

        bool TryParseDelimetedFields(string insertionFormat, char delimiter, string connector, out string result)
        {
            if (insertionFormat.Contains(delimiter))
            {
                string[] parts = insertionFormat.Split(delimiter, 2);
                string left, right;
                left = ParseField(parts[0]).Value;
                right = ParseField(parts[1]).Value;
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

        StringInsertion GetFieldByName(string name)
        {
            StringInsertion insertion;
            if (int.TryParse(name, out int id))
            {
                insertion = _attrRepo.GetInsertion(id);
            }
            else
            {
                insertion = _refRepo.GetInsertion(name);
            }
            if (insertion == null)
            {
                return new StringInsertion() { Value = "" };
            }
            return insertion;
        }
    }
}
