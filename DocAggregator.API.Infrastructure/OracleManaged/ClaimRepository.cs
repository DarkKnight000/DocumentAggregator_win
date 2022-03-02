using DocAggregator.API.Core;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    /// <summary>
    /// Реализует интерфейс <see cref="IClaimRepository"/> на основе базы данных Oracle.
    /// </summary>
    public class ClaimRepository : IClaimRepository
    {
        public Claim GetClaim(int id)
        {
            int typeID = 10;
            string template = $"Claim{typeID}.doc";
            Claim result = new Claim()
            {
                ID = id,
                Template = template,
            };
            return result;
        }
    }
}
