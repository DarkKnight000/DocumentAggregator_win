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
        /// Получает перечисление всех полей доступа используя данные определённой заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <returns>Перечисление полей прав доступа заявки.</returns>
        public IEnumerable<AccessRightField> GetFilledAccessListByClaimId(Claim claim);
    }
}
