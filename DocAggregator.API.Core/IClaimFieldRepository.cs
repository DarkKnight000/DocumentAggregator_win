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
        /// Получает значение числового или именованного поля по идентификатору заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="fieldName">Идентификатор поля.</param>
        /// <returns>Строковое представление значения поля.</returns>
        public ClaimField GetFieldByNameOrId(Claim claim, string fieldName);

        /// <summary>
        /// Получает значение права доступа для выбранного действия по идентификатору заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="roleID">Идентификатор роли.</param>
        /// <param name="status">Действие к роли.</param>
        /// <returns>Наличие действия к роли.</returns>
        public AccessRightField GetAccessRightByIdAndStatus(Claim claim, string roleID, AccessRightStatus status);

        /// <summary>
        /// Получает перечисление всех полей используя данные определённой заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <returns>Перечисление полей заявки.</returns>
        public IEnumerable<ClaimField> GetFiledListByClaimId(Claim claim);

        /// <summary>
        /// Получает перечисление всех полей доступа используя данные определённой заявки.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <returns>Перечисление полей прав доступа заявки.</returns>
        public IEnumerable<AccessRightField> GetFilledAccessListByClaimId(Claim claim);
    }
}
