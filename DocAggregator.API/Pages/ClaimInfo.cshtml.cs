﻿using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using DocAggregator.API.Infrastructure.OracleManaged;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace DocAggregator.API.Pages
{
    public class ClaimInfoModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimFieldRepository _fieldRepository;

        public Claim Claim { get; set; }
        public IEnumerable<ClaimField> ClaimFields { get; set; }
        public IEnumerable<AccessRightField> AccessFields { get; set; }
        public bool AllowAccess { get; set; }
        public bool ChangeAccess { get; set; }
        public bool DenyAccess { get; set; }
        public bool IsCorrect { get; set; } = true;

        public ClaimInfoModel(ILoggerFactory loggerFactory, IClaimRepository claimRepository, IClaimFieldRepository fieldRepository)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimInfoModel>();
            _claimRepository = claimRepository;
            _fieldRepository = fieldRepository;
        }

        public void OnGet(int? claimID)
        {
            if (!claimID.HasValue)
            {
                _logger.Warning("Claim information page was called without passing an argument.");
                IsCorrect = false;
                return;
            }
            Claim = _claimRepository.GetClaim(claimID.Value);
            ClaimFields = _fieldRepository.GetFiledListByClaimId(Claim, true);
            AccessFields = _fieldRepository.GetFilledAccessListByClaimId(Claim);
            var fullStatus = AccessFields.GetWholeStatus();
            AllowAccess = fullStatus.Equals(AccessRightStatus.Allowed);
            ChangeAccess = fullStatus.Equals(AccessRightStatus.Changed);
            DenyAccess = fullStatus.Equals(AccessRightStatus.Denied);
            return;
        }
    }
}