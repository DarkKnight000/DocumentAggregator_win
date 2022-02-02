using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class InsertResponse
    {
        public IList<Insert> Inserts;
        public InsertResponse()
        {
            Inserts = new List<Insert>();
        }
    }
}
