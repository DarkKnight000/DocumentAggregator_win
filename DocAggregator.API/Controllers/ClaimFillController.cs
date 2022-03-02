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
        public IActionResult Post([FromBody] ClaimRequest request)
        {
            var parseInteractor = new ParseInteractor(_fieldRepository);
            var formInteractor = new FormInteractor(parseInteractor, _editorService);
            var claimInteractor = new ClaimInteractor(formInteractor, _claimRepository);
            var response = claimInteractor.Handle(request);
            if (response.Success)
            {
                var presenter = new Presentation.ClaimResponseStreamPresenter();
                return presenter.Handle(response);
            }
            else
            {
                var presenter = new Presentation.InternalErrorResponsePresenter();
                return presenter.Handle(response);
            }
        }
    }
}
