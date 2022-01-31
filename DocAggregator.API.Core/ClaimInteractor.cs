using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class ClaimInteractor
    {
        public ClaimInteractor(IRepository<object> repository)
        {
            ;
        }

        public ClaimResponse Handle(ClaimRequest request)
        {
            return new ClaimResponse();
        }
    }
}
