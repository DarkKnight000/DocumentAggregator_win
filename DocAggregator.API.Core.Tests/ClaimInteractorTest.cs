using Moq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class ClaimInteractorTest
    {
        [Fact]
        public void ClaimInteractor_SuccessScenario()
        {
            // 1. Для начала заявку нужно откуда-то взять
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns(new Claim());
            // 2. Нужен драйвер для управления документом
            var mockEditorService = new Mock<IEditorService>();
            // 3. Потом отработать заполнение
            var claimInteractor = new ClaimInteractor(mockEditorService.Object, mockClaimRepository.Object);
            // 4. Получить запрос
            var request = new ClaimRequest() { ClaimID = 18012 };

            // 5. Сгенерировать документ
            var response = claimInteractor.Handle(request);

            Assert.True(response.Success);
        }

        [Fact]
        public void ClaimInteractor_NotClaimFound()
        {
            // 1. Получаем репозиторий заявок
            var mockClaimRepository = new Mock<IClaimRepository>();
            mockClaimRepository.Setup(r => r.GetClaim(It.IsAny<int>())).Returns((Claim)null);
            // 2. Нужен драйвер для управления документом
            var mockEditorService = new Mock<IEditorService>();
            // 3. Потом отработать заполнение
            var claimInteractor = new ClaimInteractor(mockEditorService.Object, mockClaimRepository.Object);
            // 4. Получить запрос
            var request = new ClaimRequest() { ClaimID = 18011 };

            // 5. Сгенерировать документ
            var response = claimInteractor.Handle(request);

            Assert.False(response.Success);
        }
    }
}
