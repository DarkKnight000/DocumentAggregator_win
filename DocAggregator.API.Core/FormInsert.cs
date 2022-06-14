using DocAggregator.API.Core.Models;
using System.Collections.Generic;

namespace DocAggregator.API.Core
{
    public class FormInsert : Insert
    {
        public List<Insert> FormFields { get; protected set; }
        public List<List<string>> FormValues { get; protected set; }

        public FormInsert(ILogger logger = null) : base(null, InsertKind.MultiField, logger)
        {
            FormFields = new List<Insert>();
            FormValues = new List<List<string>>();
        }
    }
}
