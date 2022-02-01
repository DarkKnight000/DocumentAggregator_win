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
        public async Task<FileStreamResult> Get()
        {
            var presenter = new Presentation.ClaimResponseStreamPresenter();
            return presenter.Handle(null);
        }
    }
}
