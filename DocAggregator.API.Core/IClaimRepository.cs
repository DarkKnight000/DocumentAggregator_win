using DocAggregator.API.Core.Models;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Описывает репозиторий заявок.
    /// </summary>
    public interface IClaimRepository
    {
        /// <summary>
        /// Получает данные заявки по известному идентификатору.
        /// </summary>
        /// <param name="req">Данные заявки.</param>
        /// <returns>Данные заявки.</returns>
        public Claim GetClaim(DocumentRequest req);
    }
}
