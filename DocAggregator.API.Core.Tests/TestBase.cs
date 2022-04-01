using Moq;

namespace DocAggregator.API.Core.Tests
{
    public abstract class TestBase
    {
        public ILogger Logger { get; private set; }
        public ILoggerFactory LoggerFactory { get; private set; }

        protected TestBase()
        {
            Logger = Mock.Of<ILogger>();
            LoggerFactory = Mock.Of<ILoggerFactory>(factory => factory.GetLoggerFor<object>() == Logger);
        }
    }
}
