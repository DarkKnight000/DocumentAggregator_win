using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocAggregator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimFillController : ControllerBase
    {
        private readonly ILogger<ClaimFillController> _logger;
        private readonly ClaimInteractor _claimInteractor;

        public ClaimFillController(ILogger<ClaimFillController> logger, ClaimInteractor claimInteractor)
        {
            _logger = logger;
            _claimInteractor = claimInteractor;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] ClaimRequest request)
        {
            ClaimResponse response = _claimInteractor.Handle(request);
            if (response.Success)
            {
                _logger.LogInformation("Claim was successfully processed (id={0})", request.ClaimID);
                return ClaimResponsePresenter.ToFileStreamResult(response);
            }
            else
            {
                _logger.LogWarning("Claim wasn't processed (id={0})", request.ClaimID);
                return ClaimResponsePresenter.ToErrorReport(response);
            }
        }
    }
}
