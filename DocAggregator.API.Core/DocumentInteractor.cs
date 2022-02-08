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
        ParseInteractor _parser;
        IEditorService _editor;

        public DocumentInteractor(ParseInteractor parser, IEditorService editor)
        {
            _parser = parser;
            _editor = editor;
        }

        protected override void Handle()
        {
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
                ParseResponse parseResp = _parser.Handle(parseReq);
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
