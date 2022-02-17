using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public interface IDocument { }

    public interface IEditorService<TDocument> where TDocument : IDocument
    {
        TDocument OpenTemplate(string path);
        IEnumerable<Insert> GetInserts(TDocument document);
        void SetInserts(TDocument document, IEnumerable<Insert> inserts);
        string Export(TDocument document);
    }
}
