using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public enum InsertKind
    {
        PlainText,
        CheckMark,
    }

    public class Insert
    {
        public InsertKind Kind { get; private set; }
        public string ReplacedText { get; set; }
        public bool? ReplacedCheckmark { get; set; }
        public string OriginalMask { get; set; }
        public Insert(string mask, InsertKind kind = InsertKind.PlainText)
        {
            OriginalMask = mask;
            Kind = kind;
        }
    }
}
