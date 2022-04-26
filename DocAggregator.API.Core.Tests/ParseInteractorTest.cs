using DocAggregator.API.Core.Models;
using Moq;
using System;
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
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<Claim>(), It.IsAny<string>()))
                // Returns correct output only for integers
                .Returns<Claim, string>((id, arg) => int.TryParse(arg, out _) ? new ClaimField() { Value = fieldValue } : throw new Exception());
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object, LoggerFactory);

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var response = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseInteractor_ParseField_NumberedClaimFieldNotFound()
        {
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<Claim>(), It.IsAny<string>()))
                // Returns correct output only for integers
                .Returns<Claim, string>((id, arg) => int.TryParse(arg, out _) ? null : throw new Exception());
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert("10") };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object, LoggerFactory);
            var expected = "";

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var response = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("name", "val", "val")]
        [InlineData("name", "", "")]
        public void ParseInteractor_ParseField_DenominatedField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository.Setup(r => r.GetFieldByNameOrId(It.IsAny<Claim>(), It.IsAny<string>()))
                .Returns(new ClaimField() { Value = fieldValue });
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object, LoggerFactory);

            // 4. Разбираем поле с буквенным идентификатором заявки в значение поля
            var response = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseInteractor_ParseField_DenominatedFieldNotFound()
        {
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository.Setup(r => r.GetFieldByNameOrId(It.IsAny<Claim>(), It.IsAny<string>())).Returns((ClaimField)null);
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert("name") };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object, LoggerFactory);
            var expected = "";

            // 4. Разбираем поле с буквенным идентификатором заявки в значение поля
            var response = parseInteractor.Handle(request);
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
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<Claim>(), It.IsAny<string>()))
                .Returns<Claim, string>((id, arg) => int.TryParse(arg, out _) ? new ClaimField() { Value = attrValue } : new ClaimField { Value = refValue });
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object, LoggerFactory);

            // 4. Разбираем поле со смешанным идентификатором заявки в значение поля
            var response = parseInteractor.Handle(request);
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
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<Claim>(), It.IsAny<string>()))
                // Returns only False for integers and True for strings
                .Returns<Claim, string>((id, arg) => int.TryParse(arg, out _) ? new ClaimField() { Value = "False" } : new ClaimField() { Value = "True" });
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat, InsertKind.CheckMark) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object, LoggerFactory);

            // 4. Разбираем поле с идентификатором поля заявки в значение логического поля
            var response = parseInteractor.Handle(request);
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
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository
                .Setup(r => r.GetAccessRightByIdAndStatus(It.IsAny<Claim>(), It.IsAny<string>(), It.IsAny<AccessRightStatus>()))
                .Returns<Claim, string, AccessRightStatus>((id, arg, stat) => {
                    var status = new AccessRightStatus();
                    if (anyAllow)
                    {
                        status |= AccessRightStatus.Allowed;
                    }
                    if (anyDeny)
                    {
                        status |= AccessRightStatus.Denied;
                    }
                    return new AccessRightField() { Status = status };
                });
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat, InsertKind.CheckMark) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object, LoggerFactory);

            // 4. Разбираем поле с идентификатором поля заявки в значение логического поля
            var response = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedCheckmark;

            Assert.True(actual.HasValue);
            Assert.Equal(expected, actual.Value);
        }
    }
}
