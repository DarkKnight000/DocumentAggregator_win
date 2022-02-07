using Moq;
using System.Collections.Generic;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class DocumentInteractorTest
    {
        [Fact]
        public void DocumentInteractor_SuccessScenario()
        {
            // 1. Получаем сервис редактора документов
            Mock<IEditorService> mockEditor = new Mock<IEditorService>();
            mockEditor.Setup(e => e.OpenTemplate(It.IsAny<string>())).Returns(new Document());
            mockEditor.Setup(e => e.GetInserts(It.IsAny<Document>())).Returns(new[] { new Insert("s") });
            // 2. Создаём объект запроса документа
            var request = new DocumentRequest();
            request.Claim = new Claim();
            // 3. Создаём обект интерактора документа
            var documentInteractor = new DocumentInteractor(mockEditor.Object, Mock.Of<IMixedFieldRepository>());

            // 4. Заполняем содержимое документа
            var response = documentInteractor.Handle(request);

            Assert.True(response.Success);
        }

        [Fact]
        public void DocumentInteractor_TemplateNotFound()
        {
            // 1. Получаем сервис редактора документов
            Mock<IEditorService> mockEditor = new Mock<IEditorService>();
            mockEditor.Setup(e => e.OpenTemplate(It.IsAny<string>())).Returns((Document)null);
            // 2. Создаём объект запроса документа
            var request = new DocumentRequest();
            request.Claim = new Claim();
            // 3. Создаём обект интерактора документа
            var documentInteractor = new DocumentInteractor(mockEditor.Object, Mock.Of<IMixedFieldRepository>());

            // 4. Заполняем содержимое документа
            var response = documentInteractor.Handle(request);

            Assert.False(response.Success);
        }
    }
}
