using Moq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class InsertInteractorTest
    {
        [Theory]
        [InlineData("10", "val", "val")]
        [InlineData("10", "", "")]
        public void InsertInteractor_ParseField_NumberedClaimField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns(new StringInsertion() { Value = fieldValue });
            // 2. Берём заявку
            var claim = new Claim() { ID = 0 };
            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField(insertionFormat);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InsertInteractor_ParseField_NumberedClaimFieldNotFound()
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns((StringInsertion)null);
            // 2. Берём заявку
            var claim = new Claim() { ID = 0 };
            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);
            var expected = "";

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField("10");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("name", "val", "val")]
        [InlineData("name", "", "")]
        public void InsertInteractor_ParseField_DenominatedField(string insertionFormat, string fieldValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns(new StringInsertion() { Value = fieldValue });
            // 2. Берём заявку
            var claim = new Claim() { ID = 0 };
            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле с буквенным идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField(insertionFormat);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InsertInteractor_ParseField_DenominatedFieldNotFound()
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns((StringInsertion)null);
            // 2. Берём заявку
            var claim = new Claim() { ID = 0 };
            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);
            var expected = "";

            // 4. Разбираем поле с буквенным идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField("name");

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
        public void InsertInteractor_ParseField_MixedField(string insertionFormat, string attrValue, string refValue, string expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns(new StringInsertion() { Value = attrValue });
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns(new StringInsertion() { Value = refValue });
            // 2. Берём заявку
            var claim = new Claim() { ID = 0 };
            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле со смешанным идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField(insertionFormat);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("22", false)]
        [InlineData("foi", true)]
        [InlineData("!22", true)]
        [InlineData("!foi", false)]
        public void InsertInteractor_ParseField_BooleanField(string insertionFormat, bool expected)
        {
            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns(new StringInsertion() { Value = "False" });
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns(new StringInsertion() { Value = "True" });
            // 2. Берём заявку
            var claim = new Claim() { ID = 0 };
            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            // 4. Разбираем поле с идентификатором поля заявки в значение логического поля
            var actual = insertInteractor.ParseBoolField(insertionFormat);

            Assert.Equal(expected, actual);
        }
    }
}
