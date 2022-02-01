using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class BooleanInsertion : StringInsertion
    {
        public new bool Value { get; set; }

        public static implicit operator bool(BooleanInsertion insertion)
        {
            return insertion.Value;
        }
    }
}
