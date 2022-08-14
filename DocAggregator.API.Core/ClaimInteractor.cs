using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Запрос на обработку заявки.
    /// </summary>
    public class ClaimRequest
    {
        /// <summary>
        /// Идентификатор обрабатываемой заявки.
        /// </summary>
        public int ClaimID { get; init; }

        /// <summary>
        /// IP-адрес заявителя.
        /// </summary>
        public string UserIP { get; init; }
    }

    public class DocumentRequest
    {
        public string Type { get; set; }
        public Dictionary<string, string> Args { get; set; }
    }

    /// <summary>
    /// Ответ на обработку заявки.
    /// </summary>
    public class ClaimResponse : InteractorResponseBase
    {
        /// <summary>
        /// PDF результата обработки заявки.
        /// </summary>
        public MemoryStream ResultStream { get; set; }
    }

    /// <summary>
    /// Обработчик заявки.
    /// </summary>
    public class ClaimInteractor : InteractorBase<ClaimResponse, DocumentRequest>
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

        public ClaimResponse Handle(ClaimRequest request)
        {
            return Handle(new DocumentRequest() {
                Type = "claim",
                Args = new() {
                    { "ID", request.ClaimID.ToString() },
                    { "UserIP", request.UserIP },
                },
            });
        }

        protected override void Handle(ClaimResponse response, DocumentRequest request)
        {
            Claim claim = _repo.GetClaim(request);
            if (claim == null)
            {
                throw new ArgumentException("Заявка не найдена.", nameof(request));
            }
            /*claim.ClaimFields = claim.ClaimFields.Append(new ClaimField()
            {
                VerbousID = "UserIP",
                Category = string.Empty,
                Attribute = string.Empty,
                Value = request.UserIP,
            });*/
            FormRequest formRequest = new FormRequest();
            formRequest.Claim = claim;
            FormResponse formResponse = _former.Handle(formRequest);
            if (formResponse.Success)
            {
                response.ResultStream = formResponse.ResultStream;
            }
            else
            {
                response.AddErrors(formResponse.Errors.ToArray());
            }
        }
    }
}
