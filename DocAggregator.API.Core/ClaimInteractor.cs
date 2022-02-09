using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class ClaimRequest
    {
        public int ClaimID { get; init; }
    }
    public class ClaimResponse : InteractorResponseBase
    {
        public string File { get; set; }
    }

    public class ClaimInteractor : InteractorBase<ClaimResponse, ClaimRequest>
    {
        FormInteractor _former;
        IClaimRepository _repo;

        public ClaimInteractor(FormInteractor former, IClaimRepository repository)
        {
            _former = former;
            _repo = repository;
        }

        protected override void Handle()
        {
            Claim claim = _repo.GetClaim(Request.ClaimID);
            if (claim == null)
            {
                throw new ArgumentException("Заявка не найдена.", nameof(Request.ClaimID));
            }
            FormRequest formRequest = new FormRequest();
            formRequest.Claim = claim;
            FormResponse formResponse = _former.Handle(formRequest);
            if (formResponse.Success)
            {
                Response.File = formResponse.Output;
            }
            else
            {
                Response.AddErrors(formResponse.Errors.ToArray());
            }
        }
    }
}
