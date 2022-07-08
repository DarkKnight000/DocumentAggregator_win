using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StocktakingController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly InventoryInteractor _inventoryInteractor;

        public StocktakingController(ILoggerFactory loggerFactory, InventoryInteractor inventoryInteractor)
        {
            _logger = loggerFactory.GetLoggerFor<StocktakingController>();
            _inventoryInteractor = inventoryInteractor;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post(/*[FromBody] ClaimRequest request*/)
        {
            return Ok();
            /*ClaimResponse response = _claimInteractor.Handle(request);
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
            }*/
        }
    }
}
