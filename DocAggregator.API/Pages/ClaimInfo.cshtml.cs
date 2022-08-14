using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace DocAggregator.API.Pages
{
    public class ClaimInfoModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimFieldRepository _fieldRepository;

        public Claim Claim { get; set; }
        public IEnumerable<ClaimField> ClaimFields { get; set; }
        public IEnumerable<InformationResource> InformationResources { get; set; }
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
            Claim = _claimRepository.GetClaim(new DocumentRequest() { Type = "claim", Args = { ["id"] = claimID.Value.ToString() } });
            ClaimFields = _fieldRepository.GetFiledListByClaimId(Claim, true);
            InformationResources = _fieldRepository.GetInformationalResourcesByClaim(Claim);
            var fullStatus = InformationResources.GetWholeStatus();
            AllowAccess = fullStatus.Equals(AccessRightStatus.Allow);
            ChangeAccess = fullStatus.Equals(AccessRightStatus.Change);
            DenyAccess = fullStatus.Equals(AccessRightStatus.Deny);
            return;
        }
    }
}