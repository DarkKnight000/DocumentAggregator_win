using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Controllers
{
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
            Claim claim = _repo.GetClaim(request);
            if (claim == null)
            {
                _logger.Warning("Claim wasn't processed (id={0})", request.Args["id"]);
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
