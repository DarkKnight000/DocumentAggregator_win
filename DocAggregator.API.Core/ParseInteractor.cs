using DocAggregator.API.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

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
                    insert.ReplacedCheckmark = ParseBoolField(request.Claim.Root, insert.OriginalMask);
                    break;
                case InsertKind.MultiField:
                    if (insert is FormInsert form)
                    {
                        var listedObjects = request.Claim.Root.XPathSelectElements(form.OriginalMask);
                        foreach (var item in listedObjects)
                        {
                            var line = new List<string>();
                            foreach (var field in form.FormFields)
                            {
                                line.Add(ExtractValue(item.XPathEvaluate(field.OriginalMask)));
                            }
                            form.FormValues.Add(line);
                        }
                        break;
                        /*int counter = 1;
                        switch (request.Claim.SystemID)
                        {
                            case 4: // ИС ОДФР
                                foreach (var infoResource in request.Claim.InformationResources)
                                {
                                    var accessRolesValues = infoResource.AccessRightFields.Select(
                                            (accessRole) => accessRole.Status.HasFlag(AccessRightStatus.Allow).ToString()
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
                                        status.HasFlag(AccessRightStatus.Allow).ToString(),
                                        status.HasFlag(AccessRightStatus.Deny).ToString(),
                                        role.Name,
                                    });
                                }
                                break;
                            default:
                                Logger.Warning("Claim of type {0} with ID {1} has an unknown table.", request.Claim.SystemID, request.Claim.ID);
                                break;
                        }*/
                    }
                    else
                    {
                        Logger.Warning("Expected a {0}, but have got a {1}.", typeof(FormInsert), insert.GetType());
                    }
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = ParseTextField(request.Claim.Root, insert.OriginalMask);
                    break;
            }
        }

        private void HandleInventory(ParseResponse response, ParseRequest request)
        {
            var insert = request.Insertion;
            switch (insert.Kind)
            {
                case InsertKind.CheckMark:
                    insert.ReplacedCheckmark = request.Inventory.TestBool(insert.OriginalMask);
                    break;
                case InsertKind.MultiField:
                    if (insert is FormInsert form)
                    {
                        int counter = 1;
                        foreach (var os in request.Inventory.Root.Element("OSS").Descendants())
                        {
                            var row = new List<string>();
                            foreach (var cell in form.FormFields)
                            {
                                row.Add(cell.OriginalMask == "." ? counter++.ToString() : os.Element(cell.OriginalMask.ToUpper())?.Value ?? string.Empty);
                            }
                            form.FormValues.Add(row);
                        }
                    }
                    else
                    {
                        Logger.Warning("Expected a {0}, but have got a {1}.", typeof(FormInsert), insert.GetType());
                    }
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = request.Inventory.GetField(insert.OriginalMask);
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
        /// <returns><see langword="true"/>, если значение найденного поля равно <see cref="bool.TrueString"/>, иначе <see langword="false"/>.</returns>
        bool ParseBoolField(XElement claim, string insertionFormat)
        {
            if (insertionFormat.StartsWith('*'))
            {
                return ParseAccessBoolField(claim, insertionFormat);
            }
            if (insertionFormat.StartsWith('!'))
            {
                return !ParseBoolField(claim, insertionFormat[1..]);
            }
            return bool.TryParse(claim.XPathSelectElement(insertionFormat.ToLower())?.Value, out bool result) & result;
            //return bool.TryParse(claim.Element(insertionFormat.ToUpper())?.Value, out bool result) & result;
            /*return claim.ClaimFields.Where(
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID).Equals(insertionFormat, StringComparison.OrdinalIgnoreCase)
                ).SingleOrDefault()?.ToBoolean() ?? false;*/
        }

        /// <summary>
        /// Вызывается, когда в логическом поле ожидаются данные права доступа.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns><see langword="true"/>, если значение найденного поля равно <see cref="bool.TrueString"/>, иначе <see langword="false"/>.</returns>
        bool ParseAccessBoolField(XElement claim, string insertionFormat)
        {
            string state = insertionFormat[^1..];
            insertionFormat = insertionFormat[1..^1];
            AccessRightStatus accessRight = AccessRightStatus.NotMentioned;
            switch (state)
            {
                case "a":
                    accessRight = AccessRightStatus.Allow;
                    break;
                case "c":
                    accessRight = AccessRightStatus.Change;
                    break;
                case "d":
                    accessRight = AccessRightStatus.Deny;
                    break;
            }
            if (insertionFormat == string.Empty)
            {
                return bool.TryParse(claim.XPathSelectElement(string.Format("./RESOURCES/*[1]/{0}", accessRight))?.Value, out bool res) && res;
                return claim.Element("RESOURCES").Elements().Aggregate(AccessRightStatus.NotMentioned,
                        (ars, arf) => ars | arf.Element("RIGHTS").Elements().Aggregate(AccessRightStatus.NotMentioned,
                            (ars, arf) => ars | (AccessRightStatus)Enum.Parse(typeof(AccessRightStatus), arf.Element("STATUS").Value)
                        )
                    ).Equals(accessRight);
                //return claim.InformationResources.GetWholeStatus().Equals(accessRight);
            }
            else
            {
                return bool.TryParse(claim.XPathSelectElement(string.Format("./RESOURCES/*[1]/RIGHTS/*[@index='{0}']/{1}", insertionFormat, accessRight))?.Value, out bool res) && res;
                return claim.Element("RESOURCES").Elements().Single().Element("RIGHTS").Elements().Where(
                        da => da.Attribute("index").Value == insertionFormat
                    ).SingleOrDefault()?.Element("STATUS").Value.Equals(accessRight.ToString()) ?? false;
                /*return claim.InformationResources.Single().AccessRightFields.Where(
                        arf => arf.NumeralID.ToString() == insertionFormat
                    ).SingleOrDefault()?.Status.Equals(accessRight) ?? false;*/
            }
        }

        /// <summary>
        /// Вызывается, когда в шаблоне ожидеатся текст.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns>Текстовое значение поля или пустая строка.</returns>
        string ParseTextField(XElement claim, string insertionFormat)
        {
            /*string recursiveResult;
            /*if (TryParseDelimetedFields(claim, insertionFormat, ',', ", ", out recursiveResult))
            {
                return recursiveResult;
            }
            if (TryParseDelimetedFields(claim, insertionFormat, '/', " / ", out recursiveResult))
            {
                return recursiveResult;
            }*/
            //var el = claim.XPathSelectElement(insertionFormat.ToLower());
            /*var daa = claim.XPathSelectElements(insertionFormat.ToLower());
            var faa = claim.XPathEvaluate(insertionFormat.ToLower());
            foreach (var p in daa)
            {
                p.ToString();
            }
            foreach (var d in (IEnumerable)faa)
            {
                d.ToString();
            }*/
            var a = claim.ToXPathNavigable();
            var nav = a.CreateNavigator();
            var res = nav.Evaluate(insertionFormat.ToLower());
            return ExtractValue(res);
            var attribute = claim.XPathSelectElement(string.Format("./ATTRIBUTES/*[@index='{0}']", insertionFormat))?.Value;
            //var attribute = claim.Element("ATTRIBUTES").Elements().Where((e) => e.Attribute("index").Value.Equals(insertionFormat))?.SingleOrDefault()?.Value;
            if (attribute != null)
            {
                return attribute;
            }
            var custom = claim.XPathSelectElement(string.Format("./CUSTOM/{0}", insertionFormat.ToUpper()))?.Value;
            //var custom = claim.Element("CUSTOM").Element(insertionFormat.ToUpper())?.Value;
            return custom ?? "";
            /*return claim.ClaimFields.Where(
                    cf => (cf.NumeralID?.ToString() ?? cf.VerbousID ?? "").Equals(insertionFormat, StringComparison.OrdinalIgnoreCase)
                ).SingleOrDefault()?.Value ?? "";*/
        }

        private string ExtractValue(object res)
        {
            if (res == null)
            {
                return "";
            }
            if (res is bool bul)
            {
                return bul.ToString();
            }
            if (res is double dbl)
            {
                return dbl.ToString();
            }
            if (res is string str)
            {
                return str;
            }
            if (res is XElement el)
            {
                return el.Value;
            }
            if (res is XPathNavigator nav)
            {
                return nav.Value;
            }
            if (res is IEnumerable iter) // XPathNodeIterator or compiller generated <EvaluateIterator>d_1`1
            {
                return string.Concat(iter.Cast<object>().Select((o) => ExtractValue(o)));
            }
            return "";
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
        bool TryParseDelimetedFields(XElement claim, string insertionFormat, char delimiter, string connector, out string result)
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
