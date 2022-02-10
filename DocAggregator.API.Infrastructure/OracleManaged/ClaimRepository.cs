using DocAggregator.API.Core;
using System;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class ClaimRepository : IClaimRepository
    {
        public Claim GetClaim(int id)
        {
            int typeID = 10;
            string template = $"Claim{typeID}.docx";
            Claim result = new Claim()
            {
                ID = id,
                Template = template,
            };
            return result;
        }
    }
}
