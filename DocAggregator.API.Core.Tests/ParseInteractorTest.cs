using DocAggregator.API.Core.Models;
using System.Linq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ParseInteractorTest : TestBase
    {
        [Theory]
        [InlineData("10", "val", "val")]
        [InlineData("10", "", "")]
        public void ParseInteractor_ParseField_NumberedClaimField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём заявку
            var request = new ParseRequest()
            {
                Claim = new Claim()
                {
                    ClaimFields = new[]
                    {
                        new ClaimField() { NumeralID = 10, Value = fieldValue }
                    }
                },
                Insertion = new Insert(insertionFormat)
            };
            // 2. Получаем интерактор
            var parseInteractor = new ParseInteractor(LoggerFactory);

            // 3. Разбираем поле с числовым идентификатором заявки в значение поля
            _ = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseInteractor_ParseField_NumberedClaimFieldNotFound()
        {
            // 1. Берём заявку
            var request = new ParseRequest()
            {
                Claim = new Claim()
                {
                    ClaimFields = Enumerable.Empty<ClaimField>()
                },
                Insertion = new Insert("10")
            };
            // 2. Получаем интерактор
            var parseInteractor = new ParseInteractor(LoggerFactory);
            var expected = "";

            // 3. Разбираем поле с числовым идентификатором заявки в значение поля
            _ = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("name", "val", "val")]
        [InlineData("name", "", "")]
        public void ParseInteractor_ParseField_DenominatedField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём заявку
            var request = new ParseRequest()
            {
                Claim = new Claim()
                {
                    ClaimFields = new[] { new ClaimField() { VerbousID = "name", Value = fieldValue } }
                },
                Insertion = new Insert(insertionFormat)
            };
            // 2. Получаем интерактор
            var parseInteractor = new ParseInteractor(LoggerFactory);

            // 3. Разбираем поле с буквенным идентификатором заявки в значение поля
            _ = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseInteractor_ParseField_DenominatedFieldNotFound()
        {
            // 1. Берём заявку
            var request = new ParseRequest()
            {
                Claim = new Claim()
                {
                    ClaimFields = Enumerable.Empty<ClaimField>()
                },
                Insertion = new Insert("name")
            };
            // 2. Получаем интерактор
            var parseInteractor = new ParseInteractor(LoggerFactory);
            var expected = "";

            // 3. Разбираем поле с буквенным идентификатором заявки в значение поля
            _ = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("21/name", "", "", "")]
        [InlineData("21/name", "attr", "", "attr")]
        [InlineData("21/name", "", "ref", "ref")]
        [InlineData("21/name", "attr", "ref", "attr / ref")]
        [InlineData("name/21", "attr", "ref", "ref / attr")]
        [InlineData("12/21", "attr", "", "attr / attr")]
        [InlineData("12/val/name", "attr", "ref", "attr / ref / ref")]
        [InlineData("23,name", "", "", "")]
        [InlineData("23,name", "attr", "", "attr")]
        [InlineData("23,name", "", "ref", "ref")]
        [InlineData("23,name", "attr", "ref", "attr, ref")]
        [InlineData("name,23", "attr", "ref", "ref, attr")]
        [InlineData("32,23", "attr", "", "attr, attr")]
        [InlineData("32,val,name", "attr", "ref", "attr, ref, ref")]
        public void ParseInteractor_ParseField_MixedField(string insertionFormat, string attrValue, string refValue, string expected)
        {
            // 1. Берём заявку
            var request = new ParseRequest()
            {
                Claim = new Claim()
                {
                    ClaimFields = new[]
                    {
                        new ClaimField() { NumeralID = 12, Value = attrValue },
                        new ClaimField() { NumeralID = 21, Value = attrValue },
                        new ClaimField() { NumeralID = 23, Value = attrValue },
                        new ClaimField() { NumeralID = 32, Value = attrValue },
                        new ClaimField() { VerbousID = "val", Value = refValue },
                        new ClaimField() { VerbousID = "name", Value = refValue }
                    }
                },
                Insertion = new Insert(insertionFormat)
            };
            // 2. Получаем интерактор
            var parseInteractor = new ParseInteractor(LoggerFactory);

            // 3. Разбираем поле со смешанным идентификатором заявки в значение поля
            _ = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("22", false)]
        [InlineData("foi", true)]
        [InlineData("!22", true)]
        [InlineData("!foi", false)]
        public void ParseInteractor_ParseField_BooleanField(string insertionFormat, bool expected)
        {
            // 1. Берём заявку
            var request = new ParseRequest()
            {
                Claim = new Claim()
                {
                    ClaimFields = new[]
                    {
                        new ClaimField() { NumeralID = 22, Value = "False" },
                        new ClaimField() { VerbousID = "foi", Value = "True" }
                    }
                },
                Insertion = new Insert(insertionFormat, InsertKind.CheckMark)
            };
            // 2. Получаем интерактор
            var parseInteractor = new ParseInteractor(LoggerFactory);

            // 3. Разбираем поле с идентификатором поля заявки в значение логического поля
            _ = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedCheckmark;

            Assert.True(actual.HasValue);
            Assert.Equal(expected, actual.Value);
        }

        [Theory]
        [InlineData("*a", true, false, true)]
        [InlineData("*c", true, false, false)]
        [InlineData("*d", true, false, false)]
        [InlineData("*27a", true, false, true)]
        [InlineData("*27d", true, false, false)]
        [InlineData("!*27a", true, false, false)]
        [InlineData("*a", false, true, false)]
        [InlineData("*c", false, true, false)]
        [InlineData("*d", false, true, true)]
        [InlineData("*27a", false, true, false)]
        [InlineData("*27d", false, true, true)]
        [InlineData("!*27a", false, true, true)]
        [InlineData("*c", true, true, true)]
        public void ParseInteractor_ParseRightAccessField_AllStatuses(string insertionFormat, bool anyAllow, bool anyDeny, bool expected)
        {
            // 1. Берём состояние поля заявки
            var status = new AccessRightStatus();
            if (anyAllow)
            {
                status |= AccessRightStatus.Allowed;
            }
            if (anyDeny)
            {
                status |= AccessRightStatus.Denied;
            }
            // 2. Берём заявку
            var request = new ParseRequest()
            {
                Claim = new Claim()
                {
                    InformationResources = new[]
                    {
                        new InformationResource()
                        {
                            AccessRightFields = new[] 
                            {
                                new AccessRightField()
                                {
                                    NumeralID = 27,
                                    Status = status,
                                },
                            },
                        },
                    },
                },
                Insertion = new Insert(insertionFormat, InsertKind.CheckMark),
            };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(LoggerFactory);

            // 4. Разбираем поле с идентификатором поля заявки в значение логического поля
            _ = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedCheckmark;

            Assert.True(actual.HasValue);
            Assert.Equal(expected, actual.Value);
        }
    }
}
