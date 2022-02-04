using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class ClaimInteractor
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

        public ClaimResponse Handle(ClaimRequest request)
        {
            ClaimResponse response = new ClaimResponse();
            try
            {
                Claim claim = _repo.GetClaim(request.ClaimID);
                DocumentInteractor interactor = new DocumentInteractor(_editor, _fieldRepo);
                DocumentRequest documentRequest = new DocumentRequest();
                DocumentResponse documentResponse = interactor.Handle(documentRequest);
                if (!documentResponse.Success)
                {
                    response.Errors.Concat(documentResponse.Errors);
                }
            }
            catch (Exception ex)
            {
                response.Errors.Add(ex);
            }
            return response;
        }
    }
}
