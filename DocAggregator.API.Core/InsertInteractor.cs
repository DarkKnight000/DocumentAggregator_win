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
    }
}
