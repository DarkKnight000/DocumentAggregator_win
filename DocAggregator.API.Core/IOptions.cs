namespace DocAggregator.API.Core
{
    /// <summary>
    /// Предоставляет соглашения для объектов конфигурации.
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// Получает наименование фрагмента конфигурации.
        /// </summary>
        /// <returns>Название фрагмента.</returns>
        string GetSection();
    }
}
