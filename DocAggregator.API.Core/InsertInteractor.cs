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

        public string ParseField(string insertionFormat)
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
            Insertion insertion;
            if (int.TryParse(insertionFormat, out int id))
            {
                insertion = _attrRepo.GetInsertion(id);
            }
            else
            {
                insertion = _refRepo.GetInsertion(insertionFormat);
            }
            if (insertion == null)
            {
                return "";
            }
            return insertion.Value;
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
