﻿using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
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
        private readonly ClaimInteractor _claimInteractor;

        public ClaimFillController(
            ILogger<ClaimFillController> logger,
            IEditorService editorService,
            IClaimRepository claimRepository,
            IMixedFieldRepository fieldRepository)
        {
            _logger = logger;
            ParseInteractor parseInteractor = new ParseInteractor(fieldRepository);
            FormInteractor formInteractor = new FormInteractor(parseInteractor, editorService, _logger.Adapt());
            _claimInteractor = new ClaimInteractor(formInteractor, claimRepository);
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] ClaimRequest request)
        {
            ClaimResponse response = _claimInteractor.Handle(request);
            if (response.Success)
            {
                Presentation.ClaimResponseStreamPresenter presenter = new();
                return presenter.Handle(response);
            }
            else
            {
                Presentation.InternalErrorResponsePresenter presenter = new();
                return presenter.Handle(response);
            }
        }
    }
}
