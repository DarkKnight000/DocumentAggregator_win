using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public interface IMixedFieldRepository
    {
        public string GetFieldByNameOrId(int claimID, string fieldName);
    }
}
