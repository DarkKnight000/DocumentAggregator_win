using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class DocumentInteractor
    {
        IEditorService _editor;
        IMixedFieldRepository _fieldRepo;

        public DocumentInteractor(IEditorService editor, IMixedFieldRepository mixedFieldRepository)
        {
            _editor = editor;
            _fieldRepo = mixedFieldRepository;
        }

        public DocumentResponse Handle(DocumentRequest request)
        {
            DocumentResponse response = new DocumentResponse();
            ParseInteractor parser = new ParseInteractor(_fieldRepo);
            try
            {
                IEnumerable<Insert> inserts = _editor.GetInserts();
                ParseRequest parseReq = new ParseRequest();
                foreach (Insert insert in inserts)
                {
                    parseReq.Insertion = insert;
                    ParseResponse parseResp = parser.Handle(parseReq);
                    if (!parseResp.Success)
                    {
                        response.Errors.Concat(parseResp.Errors);
                    }
                }
            }
            catch(Exception ex)
            {
                response.Errors.Add(ex);
            }
            return response;
        }
    }
}
