using DocAggregator.API.Core.Models;
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
                                line.Add(System.Web.HttpUtility.HtmlDecode(ExtractValue(item.XPathEvaluate(field.OriginalMask))));
                            }
                            form.FormValues.Add(line);
                        }
                        break;
                    }
                    else
                    {
                        Logger.Warning("Expected a {0}, but have got a {1}.", typeof(FormInsert), insert.GetType());
                    }
                    break;
                default: // InsertKind.PlainText
                    insert.ReplacedText = System.Web.HttpUtility.HtmlDecode(ParseTextField(request.Claim.Root, insert.OriginalMask));
                    break;
            }
        }

        /// <summary>
        /// Вызывается, когда в шаблоне ожидается логическое значение поля.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns><see langword="true"/>, если значение найденного поля равно <see cref="bool.TrueString"/>, иначе <see langword="false"/>.</returns>
        bool ParseBoolField(XElement claim, string insertionFormat) =>
            bool.TryParse(claim.XPathSelectElement(insertionFormat.ToLower())?.Value, out bool result) & result;

        /// <summary>
        /// Вызывается, когда в шаблоне ожидеатся текст.
        /// </summary>
        /// <param name="claim">Заявка.</param>
        /// <param name="insertionFormat">Код поля.</param>
        /// <returns>Текстовое значение поля или пустая строка.</returns>
        string ParseTextField(XElement claim, string insertionFormat)
        {
            var nav = claim.ToXPathNavigable().CreateNavigator();
            var res = nav.Evaluate(insertionFormat.ToLower());
            return ExtractValue(res);
        }

        /// <summary>
        /// Конвертирует возвращённое <see cref="XPathNavigator.Evaluate(string)"/> значение в строку.
        /// </summary>
        /// <param name="res">Result of an XPath expression.</param>
        /// <returns>String representation of the result.</returns>
        private string ExtractValue(object res)
        {
            if (res == null)
            {
                return string.Empty;
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
            return string.Empty;
        }
    }
}
