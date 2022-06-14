using DocAggregator.API.Core.Models;
using System.Collections.Generic;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Представляет репозиторий числовых и именованных полей заявки.
    /// </summary>
    public interface IClaimFieldRepository
    {
        /// <summary>
        /// Получает перечисление всех полей из общей таблицы атрибутов и представления дополнительных атрибутов по идентификатору заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="loadNames">Подгружать имена полей.</param>
        /// <returns>
        /// Полный перечень связанных с данным типом заявки атрибутами
        /// и дополными данными общего представления, основанного на данных выбранной заявки.
        /// </returns>
        public IEnumerable<ClaimField> GetFiledListByClaimId(Claim claim, bool loadNames = false);

        /// <summary>
        /// Получает перечмсление всех информационных ресурсов используя данные заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <returns>Перечисление информационных ресурсов и списком доступа для них.</returns>
        public IEnumerable<InformationResource> GetInformationalResourcesByClaim(Claim claim);

        /// <summary>
        /// Получает перечисление всех полей доступа используя данные определённой заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <returns>Перечисление полей прав доступа заявки.</returns>
        [System.Obsolete("The function should be changed to getter of an Enumerable of InformationResource.")]
        public IEnumerable<AccessRightField> GetFilledAccessListByClaimId(Claim claim);
    }
}
