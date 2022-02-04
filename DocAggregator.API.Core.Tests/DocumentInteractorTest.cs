using Moq;
using System.Collections.Generic;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class DocumentInteractorTest
    {
        [Fact]
        public void DocumentInteractor_DoDaBuDeeDaBooDa()
        {
            var request = new DocumentRequest();
            var documentInteractor = new DocumentInteractor(Mock.Of<IEditorService>(), Mock.Of<IMixedFieldRepository>());

            var response = documentInteractor.Handle(request);

            Assert.NotNull(response);
        }
    }
}
