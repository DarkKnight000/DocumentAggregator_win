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

        public ClaimFillController(ILogger<ClaimFillController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<FileStreamResult> Get([FromBody] ClaimRequest request,
            IEditorService editorService, IClaimRepository claimRepository, IMixedFieldRepository fieldRepository)
        {
            var presenter = new Presentation.ClaimResponseStreamPresenter();
            var claimInteractor = new ClaimInteractor(editorService, claimRepository);
            return presenter.Handle(claimInteractor.Handle(request));
        }
    }
}
