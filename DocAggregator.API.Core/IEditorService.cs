using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public interface IEditorService
    {
        Document OpenTemplate(string path);
        IEnumerable<Insert> GetInserts(Document document);
        void SetInserts(Document document, IEnumerable<Insert> inserts);
        string Export(Document document);
    }
}
