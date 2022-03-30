namespace DocAggregator.API.Core
{
    /// <summary>
    /// Предоставляет соглашения для фабрики конфигураций.
    /// </summary>
    public interface IOptionsFactory
    {
        /// <summary>
        /// Получает объект, представляющий конфигурацию выбранного типа.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <returns></returns>
        public TOptions GetOptionsOf<TOptions>() where TOptions : IOptions, new();
    }
}
