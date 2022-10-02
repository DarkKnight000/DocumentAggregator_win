using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Controllers
{
    /// <summary>
    /// Отвечает за генерацию документов.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DocumentFillController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ClaimInteractor _claimInteractor;

        public DocumentFillController(ILoggerFactory loggerFactory, ClaimInteractor claimInteractor)
        {
            _logger = loggerFactory.GetLoggerFor<DocumentFillController>();
            _claimInteractor = claimInteractor;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] DocumentRequest request)
        {
            DocumentResponse response = _claimInteractor.Handle(request);
            if (response.Success)
            {
                // TODO: Remove debug log.
                if (response.ResultStream != null)
                {
                    _logger.Debug("Has found a memory stream.");
                }
                _logger.Information("{0} was successfully processed (id={1})", request.Type, request.GetID());
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
                _logger.Warning("{0} wasn't processed (id={1})", request.Type, request.GetID());
                return ClaimResponsePresenter.ToErrorReport(response);
            }
        }
    }
}
