using Moq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class InsertInteractorTest
    {
        [Theory]
        [InlineData("10", "val", "val")]
        [InlineData("10", "", "")]
        public void ParseInsertInteractor_ParseField_NumberedClaimField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns(fieldValue);
            // 2. Берём заявку
            var request = new InsertRequest() { Inserts = { new Insert(insertionFormat) } };
            // 3. Получаем интерактор
            var insertInteractor = new ParseInsertInteractor(mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var response = insertInteractor.Handle(request);
            var actual = response.Inserts[0].ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseInsertInteractor_ParseField_NumberedClaimFieldNotFound()
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns((string)null);
            // 2. Берём заявку
            var request = new InsertRequest() { Inserts = { new Insert("10") } };
            // 3. Получаем интерактор
            var insertInteractor = new ParseInsertInteractor(mockAttributeRepository.Object, mockReferenceRepository.Object);
            var expected = "";

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var response = insertInteractor.Handle(request);
            var actual = response.Inserts[0].ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("name", "val", "val")]
        [InlineData("name", "", "")]
        public void ParseInsertInteractor_ParseField_DenominatedField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns(fieldValue);
            // 2. Берём заявку
            var request = new InsertRequest() { Inserts = { new Insert(insertionFormat) } };
            // 3. Получаем интерактор
            var insertInteractor = new ParseInsertInteractor(mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле с буквенным идентификатором заявки в значение поля
            var response = insertInteractor.Handle(request);
            var actual = response.Inserts[0].ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseInsertInteractor_ParseField_DenominatedFieldNotFound()
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns((string)null);
            // 2. Берём заявку
            var request = new InsertRequest() { Inserts = { new Insert("name") } };
            // 3. Получаем интерактор
            var insertInteractor = new ParseInsertInteractor(mockAttributeRepository.Object, mockReferenceRepository.Object);
            var expected = "";

            // 4. Разбираем поле с буквенным идентификатором заявки в значение поля
            var response = insertInteractor.Handle(request);
            var actual = response.Inserts[0].ReplacedText;

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
        public void ParseInsertInteractor_ParseField_MixedField(string insertionFormat, string attrValue, string refValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns(attrValue);
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns(refValue);
            // 2. Берём заявку
            var request = new InsertRequest() { Inserts = { new Insert(insertionFormat) } };
            // 3. Получаем интерактор
            var insertInteractor = new ParseInsertInteractor(mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле со смешанным идентификатором заявки в значение поля
            var response = insertInteractor.Handle(request);
            var actual = response.Inserts[0].ReplacedText;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("22", false)]
        [InlineData("foi", true)]
        [InlineData("!22", true)]
        [InlineData("!foi", false)]
        public void ParseInsertInteractor_ParseField_BooleanField(string insertionFormat, bool expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns("False");
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns("True");
            // 2. Берём заявку
            var request = new InsertRequest() { Inserts = { new Insert(insertionFormat, InsertKind.CheckMark) } };
            // 3. Получаем интерактор
            var insertInteractor = new ParseInsertInteractor(mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле с идентификатором поля заявки в значение логического поля
            var response = insertInteractor.Handle(request);
            var actual = response.Inserts[0].ReplacedCheckmark;

            Assert.True(actual.HasValue);
            Assert.Equal(expected, actual.Value);
        }
    }
}
