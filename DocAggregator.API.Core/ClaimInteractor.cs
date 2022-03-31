using System;
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
    public class ClaimInteractor : InteractorBase<ClaimResponse, ClaimRequest>
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

        protected override void Handle()
        {
            Claim claim = _repo.GetClaim(Request.ClaimID);
            if (claim == null)
            {
                throw new ArgumentException("Заявка не найдена.", nameof(Request.ClaimID));
            }
            FormRequest formRequest = new FormRequest();
            formRequest.Claim = claim;
            FormResponse formResponse = _former.Handle(formRequest);
            if (formResponse.Success)
            {
                Response.ResultStream = formResponse.ResultStream;
            }
            else
            {
                Response.AddErrors(formResponse.Errors.ToArray());
            }
        }
    }
}
