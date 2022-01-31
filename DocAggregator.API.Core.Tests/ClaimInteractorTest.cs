using DocAggregator.API.Infrastructure;
using System;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ClaimInteractorTest
    {
        [Fact]
        public void GenerationInteractor_SuccessScenario()
        {
            // arrange

            // 1. Для начала заявку нужно откуда-то взять
            var claimRepository = new ClaimRepository();

            // 2. Потом отработать заполнение
            var claimInteractor = new ClaimInteractor(claimRepository);

            // 3. Получить запрос
            var request = new ClaimRequest(18012);

            // act

            // 4. Сгенерировать документ
            var response = claimInteractor.Handle(request);

            // assert
            Assert.True(response.Success);
        }
    }
}
