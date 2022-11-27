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
    public class DocumentRequest
    {
        /// <summary>
        /// Тип обрабатываемого документа.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Инициализирующие модель поля.
        /// </summary>
        public Dictionary<string, string> Args { get; set; }

        /// <summary>
        /// Пробует получить идентификатор запрашиваемой модели.
        /// </summary>
        /// <returns>Идентификатор модели, либо литерал "unknown".</returns>
        public string GetID() => Args.ContainsKey("id") ? Args["id"] : "unknown";
    }

    /// <summary>
    /// Ответ на обработку документа.
    /// </summary>
    public class DocumentResponse : InteractorResponseBase
    {
        /// <summary>
        /// PDF результата обработки документа.
        /// </summary>
        public MemoryStream ResultStream { get; set; }

#if DEBUG

        /// <summary>
        /// Предположительное имя сгенерированного файла
        /// </summary>
        /// <remarks>
        /// Используется для удобств ручной генерации.
        /// </remarks>
        public string PresumptiveFileName { get; set; }

#endif
    }

    /// <summary>
    /// Обработчик заявки.
    /// </summary>
    public class ClaimInteractor : InteractorBase<DocumentResponse, DocumentRequest>
    {
        FormInteractor _former;
        IClaimRepository _repo;

        /// <summary>
        /// Создаёт обработчик заявки с использованием обработчика документа и репозитория заявок.
        /// </summary>
        /// <param name="former">Обработчик документов.</param>
        /// <param name="repository">Репозиторий заявок.</param>
        public ClaimInteractor(FormInteractor former, IClaimRepository repository, ILoggerFactory loggerFactory)
            : base(loggerFactory.GetLoggerFor<ClaimInteractor>())
        {
            _former = former;
            _repo = repository;
        }

        protected override void Handle(DocumentResponse response, DocumentRequest request)
        {
            Claim claim = _repo.GetClaim(request);
            if (claim == null)
            {
                throw new ArgumentException("Заявка не найдена.", nameof(request));
            }
            FormRequest formRequest = new FormRequest();
            formRequest.Claim = claim;
            FormResponse formResponse = _former.Handle(formRequest);
            if (formResponse.Success)
            {
                response.ResultStream = formResponse.ResultStream;
#if DEBUG
                if (claim.Type.ToLower() == "claim")
                {
                    var system = claim.Template.Substring(0, claim.Template.IndexOf(Path.AltDirectorySeparatorChar)).Replace(' ', '_');
                    var fullname = claim.Root?.Elements("attr").SingleOrDefault(a => a.Attribute("key")?.Value?.Equals("125") ?? false)?.Value;
                    var splittedname = fullname.Split(' ');
                    var shortname = $"{splittedname[0]}_{splittedname[1][0]}.{splittedname[2][0]}.";
                    response.PresumptiveFileName = $"{claim.ID}_{system}_{shortname}.pdf";
                }
                else
                {
                    response.PresumptiveFileName = $"{claim.Type}_id_{claim.ID}.pdf";
                }
#endif
            }
            else
            {
                response.AddErrors(formResponse.Errors.ToArray());
            }
        }
    }
}
