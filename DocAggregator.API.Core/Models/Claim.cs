using System;
using System.Xml.Linq;

namespace DocAggregator.API.Core.Models
{
    /// <summary>
    /// Представляет объект заявки.
    /// </summary>
    public class Claim
    {
        public string Type
        {
            get => Root?.Name.LocalName;
        }

        /// <summary>
        /// Шаблон соответствующий типу заявки.
        /// </summary>
        public string Template
        {
            get => Root?.Attribute("template")?.Value;
        }

        /// <summary>
        /// Идентификатор экземпляра запроса.
        /// </summary>
        public int? ID
        {
            get
            {
                if (int.TryParse(Root?.Element("ID")?.Value, out int id))
                {
                    return id;
                }
                return null;
            }
        }

        /// <summary>
        /// Корень документа
        /// </summary>
        public XElement Root { get; set; }
    }
}
