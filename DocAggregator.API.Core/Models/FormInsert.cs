using System.Collections.Generic;

namespace DocAggregator.API.Core.Models
{
    public class FormInsert : Insert
    {
        public List<Insert> FormFields { get; protected set; }
        public List<List<string>> FormValues { get; protected set; }

        public FormInsert(string mask, ILogger logger = null) : base(mask, InsertKind.MultiField, logger)
        {
            OriginalMask = mask;
            FormFields = new List<Insert>();
            FormValues = new List<List<string>>();
        }
    }
}
