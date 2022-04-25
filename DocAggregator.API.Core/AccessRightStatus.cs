using System;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Статус права доступа, указывает на применяемое к нему действие.
    /// </summary>
    [Flags]
    public enum AccessRightStatus
    {
        /// <summary>
        /// Право доступа не было упомянуто.
        /// </summary>
        NotMentioned,
        /// <summary>
        /// Запрошен доступ.
        /// </summary>
        Allowed,
        /// <summary>
        /// Запрошен отзыв.
        /// </summary>
        Denied,
        /// <summary>
        /// Запрошен и отзыв, и доступ.
        /// </summary>
        Changed,
    }
}
