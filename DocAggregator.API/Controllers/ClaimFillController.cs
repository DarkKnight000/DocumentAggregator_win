using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocAggregator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimFillController : ControllerBase
    {
        private readonly ILogger<ClaimFillController> _logger;
        private readonly IEditorService _editorService;
        private readonly IClaimRepository _claimRepository;
        private readonly IMixedFieldRepository _fieldRepository;

        public ClaimFillController(
            ILogger<ClaimFillController> logger,
            IEditorService editorService,
            IClaimRepository claimRepository,
            IMixedFieldRepository fieldRepository)
        {
            _logger = logger;
            _editorService = editorService;
            _claimRepository = claimRepository;
            _fieldRepository = fieldRepository;
        }

        [HttpPost]
        [Consumes("application/json")]
        public FileStreamResult Post([FromBody] ClaimRequest request)
        {
            var presenter = new Presentation.ClaimResponseStreamPresenter();
            var claimInteractor = new ClaimInteractor(_editorService, _claimRepository, _fieldRepository);
            return presenter.Handle(claimInteractor.Handle(request));
        }
    }
}
