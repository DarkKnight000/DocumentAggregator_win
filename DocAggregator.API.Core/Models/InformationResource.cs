using System.Collections.Generic;

namespace DocAggregator.API.Core.Models
{
    /// <summary>
    /// Представляет объект информационного ресурса.
    /// </summary>
    public class InformationResource
    {
        /// <summary>
        /// Идентификатор информационного ресурса.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Имя информационного ресурса.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Поля прав доступа заявки.
        /// </summary>
        public IEnumerable<AccessRightField> AccessRightFields { get; set; }
    }
}
