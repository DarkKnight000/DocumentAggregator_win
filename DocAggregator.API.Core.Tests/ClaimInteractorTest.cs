using Moq;
using System;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ClaimInteractorTest
    {
        [Fact]
        public void ClaimInteractor_SuccessScenario()
        {
            // arrange

            // 1. Для начала заявку нужно откуда-то взять
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns(new Claim());

            // 2. Нужен драйвер для управления документом
            var editorService = new Infrastructure.OfficeInterop.WordService();

            // 3. Потом отработать заполнение
            var claimInteractor = new ClaimInteractor(editorService, mockClaimRepository.Object);

            // 4. Получить запрос
            var request = new ClaimRequest() { ClaimID = 18012 };

            // act

            // 5. Сгенерировать документ
            var response = claimInteractor.Handle(request);

            // assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ClaimInteractor_NotClaimFound()
        {
            // arrange

            // 1. Получаем репозиторий заявок
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns((Claim)null);

            // 2. Нужен драйвер для управления документом
            var editorService = new Infrastructure.OfficeInterop.WordService();

            // 3. Потом отработать заполнение
            var claimInteractor = new ClaimInteractor(editorService, mockClaimRepository.Object);

            // 4. Получить запрос
            var request = new ClaimRequest() { ClaimID = 18011 };

            // act

            // 5. Сгенерировать документ
            var response = claimInteractor.Handle(request);

            // assert
            Assert.False(response.Success);
        }
    }
}
