using Moq;
using System.Collections.Generic;
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
            mockEditorService.Setup(s => s.GetInserts()).Returns(new[] { new Insert("id") });
            mockEditorService.Setup(s => s.SetInserts(It.IsAny<IList<Insert>>()));
            // 3. Для заполнения нужен репозиторий полей
            var mockFieldRepository = new Mock<IMixedFieldRepository>();
            mockFieldRepository.Setup(r => r.GetFieldByNameOrId(It.IsAny<string>())).Returns("text");
            // 4. Потом отработать заполнение
            var claimInteractor = new ClaimInteractor(mockEditorService.Object, mockClaimRepository.Object, mockFieldRepository.Object);
            // 5. Получить запрос
            var request = new ClaimRequest() { ClaimID = 18012 };

            // 6. Сгенерировать документ
            var response = claimInteractor.Handle(request);

            mockFieldRepository.Verify(r => r.GetFieldByNameOrId("id"));
            mockEditorService.Verify(s => s.SetInserts(new[] { new Insert("id", InsertKind.PlainText) { ReplacedText = "text" } }));
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
            mockEditorService.Setup(s => s.GetInserts()).Returns(System.Array.Empty<Insert>());
            // 3. Для заполнения нужен репозиторий полей
            var mockFieldRepository = new Mock<IMixedFieldRepository>();
            // 4. Потом отработать заполнение
            var claimInteractor = new ClaimInteractor(mockEditorService.Object, mockClaimRepository.Object, mockFieldRepository.Object);
            // 5. Получить запрос
            var request = new ClaimRequest() { ClaimID = 18011 };

            // 6. Сгенерировать документ
            var response = claimInteractor.Handle(request);

            Assert.False(response.Success);
        }
    }
}
