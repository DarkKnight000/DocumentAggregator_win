using DocAggregator.API.Infrastructure;
using Moq;
using System;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class InsertInteractorTest
    {
        [Fact]
        public void InsertInteractor_ParseField_NumberedClaimField()
        {
            // arrange

            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns(new Insertion() { Value = "da" });

            // 2. Берём заявку
            var claim = new Claim() { ID = 230 };

            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            var expected = "da";

            // act

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField("10");

            // assert

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void InsertInteractor_ParseField_NumberedClaimFieldIsEmpty()
        {
            // arrange

            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns(new Insertion() { Value = "" });

            // 2. Берём заявку
            var claim = new Claim() { ID = 230 };

            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            var expected = "";

            // act

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField("10");

            // assert

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void InsertInteractor_ParseField_NumberedClaimFieldNotFound()
        {
            // arrange

            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockAttributeRepository.Setup(r => r.GetInsertion(It.IsAny<int>())).Returns((Insertion)null);

            // 2. Берём заявку
            var claim = new Claim() { ID = 230 };

            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            var expected = "";

            // act

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField("10");

            // assert

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void InsertInteractor_ParseField_DenominatedField()
        {
            // arrange

            // 1. Берём все поля заявки
            var mockAttributeRepository = new Mock<IAttributeRepository>();
            var mockReferenceRepository = new Mock<IReferenceRepository>();
            mockReferenceRepository.Setup(r => r.GetInsertion(It.IsAny<string>())).Returns(new Insertion() { Value = "da" });

            // 2. Берём заявку
            var claim = new Claim() { ID = 230 };

            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, mockAttributeRepository.Object, mockReferenceRepository.Object);

            var expected = "da";

            // act

            // 4. Разбираем поле с буквенным идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField("name");

            // assert

            Assert.Equal(actual, expected);
        }
    }
}
