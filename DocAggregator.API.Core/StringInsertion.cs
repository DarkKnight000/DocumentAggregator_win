using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class StringInsertion
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(StringInsertion insertion)
        {
            return insertion.Value;
        }
    }
}
