using Moq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ParseInteractorTest
    {
        [Theory]
        [InlineData("10", "val", "val")]
        [InlineData("10", "", "")]
        public void ParseInteractor_ParseField_NumberedClaimField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockMixedFieldRepository = new Mock<IClaimFieldRepository>();
            mockMixedFieldRepository
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<int>(), It.IsAny<string>()))
                // Returns correct output only for integers
                .Returns<int, string>((id, arg) => int.TryParse(arg, out _) ? fieldValue : "err");
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object);

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
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<int>(), It.IsAny<string>()))
                // Returns correct output only for integers
                .Returns<int, string>((id, arg) => int.TryParse(arg, out _) ? null : "err");
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert("10") };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object);
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
            mockMixedFieldRepository.Setup(r => r.GetFieldByNameOrId(It.IsAny<int>(), It.IsAny<string>())).Returns(fieldValue);
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object);

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
            mockMixedFieldRepository.Setup(r => r.GetFieldByNameOrId(It.IsAny<int>(), It.IsAny<string>())).Returns((string)null);
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert("name") };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object);
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
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<int>(), It.IsAny<string>()))
                .Returns<int, string>((id, arg) => int.TryParse(arg, out _) ? attrValue : refValue);
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object);

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
                .Setup(r => r.GetFieldByNameOrId(It.IsAny<int>(), It.IsAny<string>()))
                // Returns only False for integers and True for strings
                .Returns<int, string>((id, arg) => int.TryParse(arg, out _) ? "False" : "True");
            // 2. Берём заявку
            var request = new ParseRequest() { Insertion = new Insert(insertionFormat, InsertKind.CheckMark) };
            // 3. Получаем интерактор
            var parseInteractor = new ParseInteractor(mockMixedFieldRepository.Object);

            // 4. Разбираем поле с идентификатором поля заявки в значение логического поля
            var response = parseInteractor.Handle(request);
            var actual = request.Insertion.ReplacedCheckmark;

            Assert.True(actual.HasValue);
            Assert.Equal(expected, actual.Value);
        }
    }
}
