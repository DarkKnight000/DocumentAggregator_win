using DocAggregator.API.Core;
using System;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class ClaimRepository : IClaimRepository
    {
        public Claim GetClaim(int id)
        {
            string template = $"Claim{id}.docx";
            Claim result = new Claim()
            {
                ID = id,
                Template = template,
            };
            return result;
        }
    }
}
