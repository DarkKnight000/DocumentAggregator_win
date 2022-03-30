namespace DocAggregator.API.Core
{
    /// <summary>
    /// Прдоставляет соглашения для фабрик ведения журнала.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Получает объект ведения журнала для заданного типа.
        /// </summary>
        /// <typeparam name="TCaller">Тип, для которого получается объект.</typeparam>
        /// <returns>Объект ведения журнала.</returns>
        ILogger GetLoggerFor<TCaller>();
    }
}
