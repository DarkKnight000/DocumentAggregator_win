using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public abstract class InteractorResponseBase
    {
        public ICollection<Exception> Errors { get; private set; }
        public bool Success => Errors.Count == 0;

        public InteractorResponseBase()
        {
            Errors = new List<Exception>();
        }
    }
}
