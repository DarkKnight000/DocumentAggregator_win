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
            Claim claim = _repo.GetClaim(request.ClaimID);
            ParseInsertInteractor parser = new ParseInsertInteractor(_fieldRepo);
            InsertRequest insertReq = new InsertRequest();
            insertReq.Inserts = _editor.GetInserts();
            InsertResponse insertResp = parser.Handle(insertReq);
            _editor.SetInserts(insertResp.Inserts);
            response.Success = claim != null;
            return response;
        }
    }
}
