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
            _logger = loggerFactory.GetLoggerFor<ClaimFillController>();
            _repo = repository;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] ClaimRequest request)
        {
            Claim claim = _repo.GetClaim(request.ClaimID);
            if (claim == null)
            {
                _logger.Warning("Claim wasn't processed (id={0})", request.ClaimID);
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
