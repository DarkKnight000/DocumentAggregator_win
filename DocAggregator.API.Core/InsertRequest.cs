using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class InsertRequest
    {
        public IList<Insert> Inserts;
        public InsertRequest()
        {
            Inserts = new List<Insert>();
        }
    }
}
