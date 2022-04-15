using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimFillController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ClaimInteractor _claimInteractor;

        public ClaimFillController(ILoggerFactory loggerFactory, ClaimInteractor claimInteractor)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimFillController>();
            _claimInteractor = claimInteractor;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] ClaimRequest request)
        {
            ClaimResponse response = _claimInteractor.Handle(request);
            if (response.Success)
            {
                // TODO: Remove debug log.
                if (response.ResultStream != null)
                {
                    _logger.Debug("Has found a memory stream.");
                }
                _logger.Information("Claim was successfully processed (id={0})", request.ClaimID);
#if DEBUG
                var outputFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), string.Concat(System.Guid.NewGuid().ToString(), ".pdf"));
                var closableStream = System.IO.File.OpenWrite(outputFile);
                response.ResultStream.CopyTo(closableStream);
                closableStream.Close();
                new System.Diagnostics.Process() {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(outputFile) { UseShellExecute = true }
                }.Start();
                return new CreatedResult(new System.Uri(outputFile), null);
#else
                return ClaimResponsePresenter.ToFileStreamResult(response);
#endif
            }
            else
            {
                _logger.Warning("Claim wasn't processed (id={0})", request.ClaimID);
                return ClaimResponsePresenter.ToErrorReport(response);
            }
        }
    }
}
