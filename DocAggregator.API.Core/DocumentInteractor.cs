using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class DocumentRequest
    {
        public Claim Claim { get; set; }
    }
    public class DocumentResponse : InteractorResponseBase
    {
        public string Output { get; set; }
    }

    public class DocumentInteractor : InteractorBase<DocumentResponse, DocumentRequest>
    {
        IEditorService _editor;
        IMixedFieldRepository _fieldRepo;

        public DocumentInteractor(IEditorService editor, IMixedFieldRepository mixedFieldRepository)
        {
            _editor = editor;
            _fieldRepo = mixedFieldRepository;
        }

        protected override void Handle()
        {
            ParseInteractor parser = new ParseInteractor(_fieldRepo);
            Document document = _editor.OpenTemplate(Request.Claim.Template);
            if (document == null)
            {
                throw new ArgumentException("Шаблон не найден.", nameof(Request.Claim.Template));
            }
            IEnumerable<Insert> inserts = _editor.GetInserts(document);
            ParseRequest parseReq = new ParseRequest();
            foreach (Insert insert in inserts)
            {
                parseReq.Insertion = insert;
                ParseResponse parseResp = parser.Handle(parseReq);
                if (!parseResp.Success)
                {
                    Response.AddErrors(parseResp.Errors.ToArray());
                }
            }
            _editor.SetInserts(document, inserts);
            Response.Output = _editor.Export(document);
        }
    }
}
