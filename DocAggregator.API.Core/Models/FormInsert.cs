using System.Collections.Generic;

namespace DocAggregator.API.Core.Models
{
    /// <summary>
    /// Вставка формы - элемент документа, имеющий список простых вставок и формирующий таблицу.
    /// </summary>
    public class FormInsert : Insert
    {
        /// <summary>
        /// Список полей формы.
        /// </summary>
        /// <remarks>
        /// Могут подразумеваться столбцы таблицы.
        /// </remarks>
        public List<Insert> FormFields { get; protected set; }

        /// <summary>
        /// Список строк со значениями для полей формы.
        /// </summary>
        /// <remarks>
        /// Могут подразумеваться строки таблицы.
        /// </remarks>
        public List<List<string>> FormValues { get; protected set; }

        public FormInsert(string mask, ILogger logger = null) : base(mask, InsertKind.MultiField, logger)
        {
            OriginalMask = mask;
            FormFields = new List<Insert>();
            FormValues = new List<List<string>>();
        }
    }
}
