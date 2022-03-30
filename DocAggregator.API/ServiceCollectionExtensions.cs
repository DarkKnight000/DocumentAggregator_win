using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace DocAggregator.API
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdapters(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactoryAdapter>();
            serviceCollection.AddSingleton<IOptionsFactory, OptionsFactoryAdapter>();
            return serviceCollection;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEditorService, Infrastructure.OpenXMLProcessing.EditorService>();
            serviceCollection.AddSingleton<IClaimRepository, Infrastructure.OracleManaged.ClaimRepository>();
            serviceCollection.AddSingleton<IMixedFieldRepository, Infrastructure.OracleManaged.MixedFieldRepository>();
            return serviceCollection;
        }

        public static IServiceCollection AddInteractors(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ParseInteractor>();
            serviceCollection.AddSingleton<FormInteractor>();
            serviceCollection.AddSingleton<ClaimInteractor>();
            return serviceCollection;
        }
    }
}
