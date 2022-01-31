using DocAggregator.API.Infrastructure;
using System;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class AttributeInteractorTest
    {
        [Fact]
        public void ClaimInteractor_ParseField_NumberedClaimField()
        {
            // arrange

            // 1. Берём все поля заявки
            var attributeRepository = new AttributeRepository();

            // 2. Берём заявку
            var claim = new Claim() { ID = 230 };

            // 3. Получаем интерактор
            var insertInteractor = new InsertInteractor(claim, attributeRepository);

            var expected = "da";

            // act

            // 4. Разбираем поле с числовым идентификатором заявки в значение поля
            var actual = insertInteractor.ParseField("10");

            // assert

            Assert.Equal(actual, expected);
        }
    }
}
