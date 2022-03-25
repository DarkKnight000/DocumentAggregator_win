using Moq;

namespace DocAggregator.API.Core.Tests
{
    public abstract class TestBase
    {
        public ILogger Logger { get; private set; }

        protected TestBase()
        {
            Logger = Mock.Of<ILogger>();
        }
    }
}
