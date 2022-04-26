using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Запрос на обработку документа.
    /// </summary>
    public class FormRequest
    {
        /// <summary>
        /// Заявка, связанная с документом.
        /// </summary>
        public Claim Claim { get; set; }
    }

    /// <summary>
    /// Ответ на обработку документа.
    /// </summary>
    public class FormResponse : InteractorResponseBase
    {
        /// <summary>
        /// PDF результата обработки, хранимый в памяти.
        /// </summary>
        public MemoryStream ResultStream { get; set; }
    }

    /// <summary>
    /// Обработчик документа.
    /// </summary>
    public class FormInteractor : InteractorBase<FormResponse, FormRequest>
    {
        ParseInteractor _parser;
        IEditorService _editor;

        /// <summary>
        /// Создаёт обработчик документа с использованием обработчика вставки и редактора документа.
        /// </summary>
        /// <param name="parser">Обработчик вставки.</param>
        /// <param name="editor">Редактор документа.</param>
        /// <param name="loggerFactory">Фабрика журналов.</param>
        public FormInteractor(ParseInteractor parser, IEditorService editor, ILoggerFactory loggerFactory)
            : base(loggerFactory.GetLoggerFor<FormInteractor>())
        {
            _parser = parser;
            _editor = editor;
        }

        protected override void Handle(FormResponse response, FormRequest request)
        {
            IDocument document = null;
            try
            {
                document = _editor.OpenTemplate(request.Claim.Template);
            }
            // Шаблон не найден.
            catch (FileNotFoundException ex)
            {
                response.Errors.Add(ex);
            }
            // System.Runtime.InteropServices.COMException:
            // Приложению Word не удалось прочитать документ. Возможно, он поврежден.
            catch (Exception ex)
            {
                Logger.Error(ex, "Непридвиденная ошибка обнаружена при попытке открыть и прочитать шаблон.");
                response.Errors.Add(ex);
            }
            if (document == null)
            {
                string errorMessage = string.Format("Ошибка при открытии шаблона по пути {0}.",
                    Path.Combine(_editor.TemplatesDirectory, request.Claim.Template));
                throw new ArgumentException(errorMessage, nameof(request.Claim.Template));
            }
            IEnumerable<Insert> inserts = _editor.GetInserts(document).ToList();
            ParseRequest parseReq = new ParseRequest();
            parseReq.Claim = request.Claim;
            foreach (Insert insert in inserts)
            {
                parseReq.Insertion = insert;
                ParseResponse parseResp = _parser.Handle(parseReq);
                if (!parseResp.Success)
                {
                    response.AddErrors(parseResp.Errors.ToArray());
                }
            }
            _editor.SetInserts(document, inserts);
            response.ResultStream = _editor.Export(document) as MemoryStream;
        }
    }
}
