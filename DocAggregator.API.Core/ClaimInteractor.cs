using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Декодирует каждый элемент поля <see cref="Args"/>, зашифрованных BASE64, в кодировку платформы по-умолчанию.
        /// </summary>
        /// <remarks>
        /// При ошибке декодирования, пропускает элемент и продолжает со следующего.
        /// </remarks>
        /// <returns>Успешность выполнения преобразования для словаря в целом.</returns>
        public bool TryDecodeArgs(Encoding encoding = null, ILogger logger = null)
        {
            Encoding _encoding = encoding ?? Encoding.Default;
            bool success = true;
            byte[] buffer;
            foreach (var key in Args.Keys)
            {
                var argValue = Args[key];
                buffer = new byte[argValue.Length];
                if (Convert.TryFromBase64String(argValue, buffer, out int bytesWritten))
                {
                    var decodedArg = _encoding.GetString(buffer, 0, bytesWritten);
                    logger?.Trace("{0} was converted from \"{1}\" to \"{2}\"", key, argValue, decodedArg);
                    Args[key] = decodedArg;
                }
                else
                {
                    logger?.Warning("{0} wasn't decoded, value is \"{1}\"", key, argValue);
                    success = false;
                }
            }
            return success;
        }
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
