using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Запрос на разбор вставки.
    /// </summary>
    public class ParseRequest
    {
        /// <summary>
        /// Идентификатор заявки.
        /// </summary>
        public Claim Claim { get; set; }

        public Inventory Inventory { get; set; }

        /// <summary>
        /// Вставка, изменяемая в процессе обработки.
        /// </summary>
        public Insert Insertion { get; set; }
    }

    /// <summary>
    /// Ответ на разбор вставки.
    /// </summary>
    public class ParseResponse : InteractorResponseBase { }

    /// <summary>
    /// Обработчик разбора вставки.
    /// </summary>
    public class ParseInteractor : InteractorBase<ParseResponse, ParseRequest>
    {
        /// <summary>
        /// Создаёт обработчик на основе репозитория полей заявки.
        /// </summary>
        /// <param name="fieldRepository">Репозиторий полей заявки.</param>
        public ParseInteractor(ILoggerFactory loggerFactory)
            : base(loggerFactory.GetLoggerFor<ParseInteractor>()) { }

        protected override void Handle(ParseResponse response, ParseRequest request)
        {
            if (request.Claim != null)
            {
                HandleClaim(response, request);
            }
            else if (request.Inventory != null)
            {
                HandleInventory(response, request);
            }
            else
            {
                throw new ArgumentNullException(nameof(request), "Request has no valid object to process.");
            }
        }

        private void HandleClaim(ParseResponse response, ParseRequest request)
        {
            var insert = request.Insertion;
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    insert.ReplacedCheckmark = ParseBoolField(request.Claim, insert.OriginalMask);
                    break;
                case InsertKind.MultiField:
                    if (insert is FormInsert form)
                    {
                        int counter = 1;
                        switch (request.Claim.SystemID)
                        {
                            case 4: // ИС ОДФР
                                foreach (var infoResource in request.Claim.InformationResources)
                                {
                                    var accessRolesValues = infoResource.AccessRightFields.Select(
                                            (accessRole) => accessRole.Status.HasFlag(AccessRightStatus.Allowed).ToString()
                                        ).ToArray();
                                    form.FormValues.Add(new List<string>() {
                                        counter++.ToString(),
                                        infoResource.Name,
                                        accessRolesValues[0],
                                        accessRolesValues[1],
                                        accessRolesValues[2],
                                    });
                                }
                                break;
                            case 17: // ИС ОиК
                                foreach (var role in request.Claim.InformationResources.First().AccessRightFields)
                                {
                                    var status = request.Claim.InformationResources.Aggregate(AccessRightStatus.NotMentioned,
                                            (ars, res) => ars | res.AccessRightFields.Where(
                                                    (rf) => rf.Name.Equals(role.Name)
                                                ).Select(
                                                    (rf) => rf.Status
                                                ).SingleOrDefault()
                                        );
                                    form.FormValues.Add(new List<string>() {
                                        status.HasFlag(AccessRightStatus.Allowed).ToString(),
                                        status.HasFlag(AccessRightStatus.Denied).ToString(),
                                        role.Name,
                                    });
                                }
                                break;
                            default:
                                Logger.Warning("Claim of type {0} with ID {1} has an unknown table.", request.Claim.SystemID, request.Claim.ID);
                                break;
                        }
                    }
                    else
                    {
                        Logger.Warning("Expected a {0}, but have got a {1}.", typeof(FormInsert), insert.GetType());
                    }
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = ParseTextField(request.Claim, insert.OriginalMask);
                    break;
            }
        }

        private void HandleInventory(ParseResponse response, ParseRequest request)
        {
            var insert = request.Insertion;
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    insert.ReplacedCheckmark = request.Inventory.InventoryFields.SingleOrDefault((field) => field.VerbousID == insert.OriginalMask)?.ToBoolean() ?? false;
                    break;
                case InsertKind.MultiField:
                    if (insert is FormInsert form)
                    {
                        int counter = 1;
                        foreach (var infoResource in Enumerable.Range(0, 6))
                        {
                            form.FormValues.Add(new List<string>() {
                                counter++.ToString(),
                                "net",
                                "pc",
                                infoResource.ToString(),
                            });
                        }
                    }
                    else
                    {
                        Logger.Warning("Expected a {0}, but have got a {1}.", typeof(FormInsert), insert.GetType());
                    }
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = request.Inventory.InventoryFields.SingleOrDefault((field) => field.VerbousID == insert.OriginalMask)?.Value ?? "";
                    break;
            }
        }

        /// <summary>
        /// Вызывается, когда в шаблоне ожидается логическое значение поля.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <remarks>
        /// Значение может быть инвертировано ведущим символом '!' в коде поля.
        /// </remarks>
        /// <returns>true, если значение найденного поля равно <see cref="bool.TrueString"/>, иначе false.</returns>
        bool ParseBoolField(Claim claim, string insertionFormat)
        {
            if (insertionFormat.StartsWith('*'))
            {
                return ParseAccessBoolField(claim, insertionFormat);
            }
            if (insertionFormat.StartsWith('!'))
            {
                return !ParseBoolField(claim, insertionFormat[1..]);
            }
            return claim.ClaimFields.Where(
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID).Equals(insertionFormat, StringComparison.OrdinalIgnoreCase)
                ).SingleOrDefault()?.ToBoolean() ?? false;
        }

        /// <summary>
        /// Вызывается, когда в логическом поле ожидаются данные права доступа.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns>true, если значение найденного поля равно <see cref="bool.TrueString"/>, иначе false.</returns>
        bool ParseAccessBoolField(Claim claim, string insertionFormat)
        {
            string state = insertionFormat[^1..];
            insertionFormat = insertionFormat[1..^1];
            AccessRightStatus accessRight = AccessRightStatus.NotMentioned;
            switch (state)
            {
                case "a":
                    accessRight = AccessRightStatus.Allowed;
                    break;
                case "c":
                    accessRight = AccessRightStatus.Changed;
                    break;
                case "d":
                    accessRight = AccessRightStatus.Denied;
                    break;
            }
            if (insertionFormat == string.Empty)
            {
                return claim.InformationResources.GetWholeStatus().Equals(accessRight);
            }
            else
            {
                return claim.InformationResources.Single().AccessRightFields.Where(
                        arf => arf.NumeralID.ToString() == insertionFormat
                    ).SingleOrDefault()?.Status.Equals(accessRight) ?? false;
            }
        }

        /// <summary>
        /// Вызывается, когда в шаблоне ожидеатся текст.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns>Текстовое значение поля или пустая строка.</returns>
        string ParseTextField(Claim claim, string insertionFormat)
        {
            string recursiveResult;
            if (TryParseDelimetedFields(claim, insertionFormat, ',', ", ", out recursiveResult))
            {
                return recursiveResult;
            }
            if (TryParseDelimetedFields(claim, insertionFormat, '/', " / ", out recursiveResult))
            {
                return recursiveResult;
            }
            return claim.ClaimFields.Where(
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID ?? "").Equals(insertionFormat, StringComparison.OrdinalIgnoreCase)
                ).SingleOrDefault()?.Value ?? "";
        }

        /// <summary>
        /// Позволяет форматировать неколько полей заявки в одном поле.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <param name="delimiter">Искомый разделитель полей.</param>
        /// <param name="connector">Соединитель значений полей.</param>
        /// <param name="result">Результат рекурсивного разрешения кода.</param>
        /// <returns>Значение, указывающее на успешность операции.</returns>
        bool TryParseDelimetedFields(Claim claim, string insertionFormat, char delimiter, string connector, out string result)
        {
            if (insertionFormat.Contains(delimiter))
            {
                string[] parts = insertionFormat.Split(delimiter, 2);
                string left, right;
                left = ParseTextField(claim, parts[0]);
                right = ParseTextField(claim, parts[1]);
                if (left == string.Empty || right == string.Empty)
                {
                    result = left + right;
                }
                else
                {
                    result = left + connector + right;
                }
                return true;
            }
            result = null;
            return false;
        }
    }
}
