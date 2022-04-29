using DocAggregator.API.Core.Models;
using System.Collections.Generic;

namespace DocAggregator.API.Core
{
    [System.Obsolete("Прототип обработчика формы, не завершён.")]
    public class FormInsert : Insert
    {
        public List<Insert> FormFields { get; protected set; }

        public FormInsert(ILogger logger = null) : base(null, InsertKind.MultiField, logger)
        {
        }
    }
}
