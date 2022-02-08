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
        DocumentInteractor _creator;
        IClaimRepository _repo;

        public ClaimInteractor(DocumentInteractor creator, IClaimRepository repository)
        {
            _creator = creator;
            _repo = repository;
        }

        protected override void Handle()
        {
            Claim claim = _repo.GetClaim(Request.ClaimID);
            if (claim == null)
            {
                throw new ArgumentException("Заявка не найдена.", nameof(Request.ClaimID));
            }
            DocumentRequest documentRequest = new DocumentRequest();
            documentRequest.Claim = claim;
            DocumentResponse documentResponse = _creator.Handle(documentRequest);
            if (documentResponse.Success)
            {
                Response.File = documentResponse.Output;
            }
            else
            {
                Response.AddErrors(documentResponse.Errors.ToArray());
            }
        }
    }
}
