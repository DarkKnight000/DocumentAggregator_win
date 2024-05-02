using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using DocAggregator.API.Infrastructure;
using DocAggregator.API.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DocAggregator.API.Infrastructure.OracleManaged;
using System.IO;
using System;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text;
using asd;

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
            FormInteractor.GetType = request.Type;
            if (!request.TryDecodeArgs(encoding: System.Text.Encoding.GetEncoding(1251), logger: _logger))
            {
                _logger.Warning("Not all arguments was decoded from BASE64.");
            }
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
                var outputFile = Path.Combine(Path.GetTempPath(), response.PresumptiveFileName);
                var closableStream = System.IO.File.OpenWrite(outputFile);
                response.ResultStream.CopyTo(closableStream);
                closableStream.Close();
                // Открывает созданный pdf в браузере
                /*new Process() {
                    StartInfo = new ProcessStartInfo(outputFile) { UseShellExecute = true }
                }.Start();*/
                return new CreatedResult(new Uri(outputFile), null);
#else
                
                
                _logger.Debug("+=+=+=+=++=+=+=+=++=+=+=+=++=+=+=+=+");
                return ClaimResponsePresenter.ToFileStreamResult(response);

#endif
            }
            else
            {
                _logger.Warning("{0} wasn't processed (id={1})", request.Type, request.GetID());
                //return Redirect("/Result");
                return ClaimResponsePresenter.ToErrorReport(response);

            }
        }
    }
}
