using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace DocAggregator.API
{
    /// <summary>
    /// Содержит наборы комманд конфигураций коллекции сервисов.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет фабрики-адаптеры для систем и сервисов платформы.
        /// </summary>
        /// <returns>Та же коллекция сервисов, переданная в метод расширения.</returns>
        public static IServiceCollection AddAdapters(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactoryAdapter>();
            serviceCollection.AddSingleton<IOptionsFactory, OptionsFactoryAdapter>();
            return serviceCollection;
        }

        /// <summary>
        /// Добавляет внешние зависимости для ядра приложения.
        /// </summary>
        /// <returns>Та же коллекция сервисов, переданная в метод расширения.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Infrastructure.OracleManaged.SqlConnectionResource>();
            serviceCollection.AddSingleton<Infrastructure.OracleManaged.TemplateMap>();

            serviceCollection.AddSingleton<IEditorService, Infrastructure.OpenXMLProcessing.EditorService>();
            serviceCollection.AddSingleton<IClaimRepository, Infrastructure.OracleManaged.ClaimRepository>();
            serviceCollection.AddSingleton<IClaimFieldRepository, Infrastructure.OracleManaged.MixedFieldRepository>();
            return serviceCollection;
        }

        /// <summary>
        /// Добавляет обработчики ядра приложения.
        /// </summary>
        /// <returns>Та же коллекция сервисов, переданная в метод расширения.</returns>
        public static IServiceCollection AddInteractors(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ParseInteractor>();
            serviceCollection.AddSingleton<FormInteractor>();
            serviceCollection.AddSingleton<ClaimInteractor>();
            return serviceCollection;
        }
    }
}
