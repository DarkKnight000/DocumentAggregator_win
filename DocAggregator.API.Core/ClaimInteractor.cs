using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class ClaimInteractor
    {
        IClaimRepository _repo;

        public ClaimInteractor(IEditorService editor, IClaimRepository repository)
        {
            _repo = repository;
        }

        public ClaimResponse Handle(ClaimRequest request)
        {
            ClaimResponse response = new ClaimResponse();
            Claim claim = _repo.GetClaim(request.ClaimID);
            response.Success = claim != null;
            return response;
        }
    }
}
