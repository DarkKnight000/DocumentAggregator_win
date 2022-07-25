using System.Collections.Generic;
using System.Xml.Linq;

namespace DocAggregator.API.Core.Models
{
    /// <summary>
    /// Представляет объект заявки.
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Идентификатор заявки, согласно базе данных.
        /// </summary>
        public int ID { get; init; }

        /// <summary>
        /// Идентификатор типа заявки.
        /// </summary>
        public int TypeID { get; init; }

        /// <summary>
        /// Идентификатор системы, к которой зарашивается доступ.
        /// </summary>
        public int SystemID { get; init; }

        /// <summary>
        /// Шаблон соответствующий типу заявки.
        /// </summary>
        public string Template { get; init; }

        public XElement Root { get; set; }

        /// <summary>
        /// Поля заявки.
        /// </summary>
        public IEnumerable<ClaimField> ClaimFields { get; set; }

        /// <summary>
        /// Список затронутых информационных ресурсов.
        /// </summary>
        public IEnumerable<InformationResource> InformationResources { get; set; }
    }
}
