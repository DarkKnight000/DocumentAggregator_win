using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public interface IEditorService
    {
        public Document OpenTemplate(string path);
        public IEnumerable<Insert> GetInserts(Document document);
        public void SetInserts(Document document, IEnumerable<Insert> inserts);
    }
}
