using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using DocAggregator.API.Presentation;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DocAggregator.API.Controllers
{
    /// <summary>
    /// Позволяет получить структуру и содержание модели документа.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DocumentStructureController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IClaimRepository _repo;

        public DocumentStructureController(ILoggerFactory loggerFactory, IClaimRepository repository)
        {
            _logger = loggerFactory.GetLoggerFor<DocumentStructureController>();
            _repo = repository;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] DocumentRequest request)
        {
            Claim claim;
            try
            {
                claim = _repo.GetClaim(request);
            }
            catch (Exception ex)
            {
                return ClaimResponsePresenter.ToErrorReport(new DocumentResponse() { Errors = { ex } });
            }
            if (claim == null)
            {
                _logger.Warning("{0} wasn't processed (id={1})", request.Type, request.GetID());
                return new ObjectResult("A problem has met.")
                {
                    StatusCode = 500,
                };
            }
            else
            {
                return new ContentResult()
                {
                    Content = claim.Root.ToString(),
                    ContentType = "application/xml",
                    StatusCode = 200,
                };
            }
        }
    }
}
