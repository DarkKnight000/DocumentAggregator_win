using DocAggregator.API.Core.Models;
using Moq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class FormInteractorTest : TestBase
    {
        [Fact]
        public void FormInteractor_SuccessScenario()
        {
            // 1. Получаем сервис редактора документов
            Mock<IEditorService> mockEditor = new Mock<IEditorService>();
            mockEditor.Setup(e => e.OpenTemplate(It.IsAny<string>())).Returns(Mock.Of<IDocument>);
            mockEditor.Setup(e => e.GetInserts(It.IsAny<IDocument>())).Returns(new[] { new Insert("s") });
            // 2. Создаём объект запроса документа
            var request = new FormRequest();
            request.Claim = new Claim();
            // 3. Создаём обект интерактора документа
            var formInteractor = new FormInteractor(new Mock<ParseInteractor>(LoggerFactory).Object, mockEditor.Object, LoggerFactory);

            // 4. Заполняем содержимое документа
            var response = formInteractor.Handle(request);

            Assert.True(response.Success, ResponseDebugPresenter.Handle(response));
        }

        [Fact]
        public void FormInteractor_TemplateNotFound()
        {
            // 1. Получаем сервис редактора документов
            Mock<IEditorService> mockEditor = new Mock<IEditorService>();
            mockEditor.Setup(e => e.OpenTemplate(It.IsAny<string>())).Returns((IDocument)null);
            // 2. Создаём объект запроса документа
            var request = new FormRequest();
            request.Claim = new Claim();
            // 3. Создаём обект интерактора документа
            var formInteractor = new FormInteractor(new Mock<ParseInteractor>(LoggerFactory).Object, mockEditor.Object, LoggerFactory);

            // 4. Заполняем содержимое документа
            var response = formInteractor.Handle(request);

            Assert.False(response.Success);
        }
    }
}
