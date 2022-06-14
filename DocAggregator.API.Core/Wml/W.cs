using System.Xml.Linq;

namespace DocAggregator.API.Core.Wml
{
    /// <summary>
    /// Contains the newer Word XML names.
    /// </summary>
    public static class W14
    {
        /// <summary>
        /// The newer Word namespace.
        /// </summary>
        public static readonly XNamespace w14 = "http://schemas.microsoft.com/office/word/2010/wordml";

        /// <summary>
        /// Check box content control type.
        /// </summary>
        public static readonly XName checkbox = w14 + "checkbox";
        /// <summary>
        /// Value node of a check box.
        /// </summary>
        public static readonly XName check = w14 + "checked";
        /// <summary>
        /// Numeral value attribute of <see cref="check"/> node.
        /// </summary>
        public static readonly XName val = w14 + "val";
    }

    /// <summary>
    /// Contains the Word XML names.
    /// </summary>
    public static class W
    {
        /// <summary>
        /// The Word namespace.
        /// </summary>
        public static readonly XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>
        /// Structured document tag.
        /// </summary>
        public static readonly XName sdt = w + "sdt";
        /// <summary>
        /// Structured document tag properties.
        /// </summary>
        public static readonly XName sdtPr = w + "sdtPr";
        /// <summary>
        /// Tag of a structured document tag.
        /// </summary>
        public static readonly XName tag = w + "tag";
        /// <summary>
        /// Title of a structured document tag.
        /// </summary>
        public static readonly XName alias = w + "alias";
        /// <summary>
        /// Value.
        /// </summary>
        public static readonly XName val = w + "val";
        /// <summary>
        /// Structured document tag content.
        /// </summary>
        public static readonly XName sdtContent = w + "sdtContent";
        /// <summary>
        /// Paragraph.
        /// </summary>
        public static readonly XName p = w + "p";
        /// <summary>
        /// Run.
        /// </summary>
        public static readonly XName r = w + "r";
        /// <summary>
        /// Run properties.
        /// </summary>
        public static readonly XName rPr = w + "rPr";
        /// <summary>
        /// Run style.
        /// </summary>
        public static readonly XName rStyle = w + "rStyle";
        /// <summary>
        /// Text.
        /// </summary>
        public static readonly XName t = w + "t";
        /// <summary>
        /// Table.
        /// </summary>
        public static readonly XName tbl = w + "tbl";
        /// <summary>
        /// Table row.
        /// </summary>
        public static readonly XName tr = w + "tr";
        /// <summary>
        /// Table cell.
        /// </summary>
        public static readonly XName tc = w + "tc";
        /// <summary>
        /// Text field content control type.
        /// </summary>
        public static readonly XName text = w + "text";
    }
}
