using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public class FormRequest
    {
        public Claim Claim { get; set; }
    }
    public class FormResponse : InteractorResponseBase
    {
        public string Output { get; set; }
    }

    public class FormInteractor : InteractorBase<FormResponse, FormRequest>
    {
        ParseInteractor _parser;
        IEditorService _editor;

        public FormInteractor(ParseInteractor parser, IEditorService editor)
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
            IEnumerable<Insert> inserts = _editor.GetInserts(document).ToList();
            ParseRequest parseReq = new ParseRequest();
            parseReq.ClaimID = Request.Claim.ID;
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
