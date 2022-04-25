using System;
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
        /// <param name="claimID">Идентификатор заявки.</param>
        /// <param name="fieldName">Идентификатор поля.</param>
        /// <returns>Строковое представление значения поля.</returns>
        public string GetFieldByNameOrId(int claimID, string fieldName);

        /// <summary>
        /// Получает значение права доступа для выбранного действия по идентификатору заявки.
        /// </summary>
        /// <param name="claimID">Идентификатор заявки.</param>
        /// <param name="roleID">Идентификатор роли.</param>
        /// <param name="status">Действие к роли.</param>
        /// <returns>Наличие действия к роли.</returns>
        public bool GetAccessRightByIdAndStatus(int claimID, string roleID, AccessRightStatus status);

        /// <summary>
        /// Получает перечисление всех полей используя данные определённой заявки.
        /// </summary>
        /// <param name="claimID">Идентификатор заявки</param>
        /// <returns>Кортеж, содержащий имя, идентификатор и значение поля.</returns>
        public IEnumerable<Tuple<string, string, string>> GetFiledListByClaimId(int claimID);

        public IEnumerable<Tuple<string, string, bool?>> GetFilledAccessListByClaimId(int claimID);
    }
}
