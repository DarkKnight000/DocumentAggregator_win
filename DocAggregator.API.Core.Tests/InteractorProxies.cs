using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core.Tests
{
    public class DocumentInteractorProxy : FormInteractor
    {
        public DocumentInteractorProxy() : base(null, null) { }
    }

    public class ParseInteractorProxy : ParseInteractor
    {
        public ParseInteractorProxy() : base(null) { }
    }
}
