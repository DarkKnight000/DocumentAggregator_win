namespace DocAggregator.API.Core
{
    public interface ILoggerFactory
    {
        ILogger GetLoggerFor<TCaller>();
    }
}
