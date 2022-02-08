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
        IEditorService _editor;
        IClaimRepository _repo;
        IMixedFieldRepository _fieldRepo;

        public ClaimInteractor(IEditorService editor, IClaimRepository repository, IMixedFieldRepository fieldRepository)
        {
            _editor = editor;
            _repo = repository;
            _fieldRepo = fieldRepository;
        }

        protected override void Handle()
        {
            Claim claim = _repo.GetClaim(Request.ClaimID);
            if (claim == null)
            {
                throw new ArgumentException("Заявка не найдена.", nameof(Request.ClaimID));
            }
            DocumentInteractor interactor = new DocumentInteractor(_editor, _fieldRepo);
            DocumentRequest documentRequest = new DocumentRequest();
            documentRequest.Claim = claim;
            DocumentResponse documentResponse = interactor.Handle(documentRequest);
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
