using Moq;
using System.Collections.Generic;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class FormInteractorTest
    {
        [Fact]
        public void FormInteractor_SuccessScenario()
        {
            // 1. Получаем сервис редактора документов
            Mock<IEditorService> mockEditor = new Mock<IEditorService>();
            mockEditor.Setup(e => e.OpenTemplate(It.IsAny<string>())).Returns(new Document());
            mockEditor.Setup(e => e.GetInserts(It.IsAny<Document>())).Returns(new[] { new Insert("s") });
            // 2. Создаём объект запроса документа
            var request = new FormRequest();
            request.Claim = new Claim();
            // 3. Создаём обект интерактора документа
            var formInteractor = new FormInteractor(Mock.Of<ParseInteractorProxy>(), mockEditor.Object);

            // 4. Заполняем содержимое документа
            var response = formInteractor.Handle(request);

            Assert.True(response.Success, ResponseDebugPresenter.Handle(response));
        }

        [Fact]
        public void FormInteractor_TemplateNotFound()
        {
            // 1. Получаем сервис редактора документов
            Mock<IEditorService> mockEditor = new Mock<IEditorService>();
            mockEditor.Setup(e => e.OpenTemplate(It.IsAny<string>())).Returns((Document)null);
            // 2. Создаём объект запроса документа
            var request = new FormRequest();
            request.Claim = new Claim();
            // 3. Создаём обект интерактора документа
            var formInteractor = new FormInteractor(Mock.Of<ParseInteractorProxy>(), mockEditor.Object);

            // 4. Заполняем содержимое документа
            var response = formInteractor.Handle(request);

            Assert.False(response.Success);
        }
    }
}
