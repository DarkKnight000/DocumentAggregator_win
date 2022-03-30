using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocAggregator.API
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection serviceProvider, IConfiguration configuration)
        {
            serviceProvider.AddSingleton<IEditorService>((s) =>
            {
                var editorService = new Infrastructure.OpenXMLProcessing.EditorService(s.GetService<ILogger<IEditorService>>().Adapt())
                {
                    TemplatesDirectory = configuration["Editor:TemplatesDir"],
                    TemporaryOutputDirectory = configuration["Editor:OutputDir"],
                    LibreOfficeFolder = configuration["Editor:LibreOffice"],
                    Scripts = configuration["Editor:Scripts"],
                };
                editorService.Initialize();
                return editorService;
            });
            serviceProvider.AddSingleton<IClaimRepository, Infrastructure.OracleManaged.ClaimRepository>();
            serviceProvider.AddSingleton<IMixedFieldRepository>((s) =>
            {
                var fieldRepository = new Infrastructure.OracleManaged.MixedFieldRepository(s.GetService<ILogger<IMixedFieldRepository>>().Adapt())
                {
                    QueriesSource = configuration["DB:QueriesFile"],
                    Server = configuration["DB:DataSource"],
                    Username = configuration["DB:UserID"],
                    Password = configuration["DB:Password"],
                };
                return fieldRepository;
            });
            return serviceProvider;
        }

        public static IServiceCollection AddInteractors(this IServiceCollection serviceProvider)
        {
            serviceProvider.AddSingleton<ParseInteractor>();
            serviceProvider.AddSingleton<FormInteractor>((s) => {
                return new FormInteractor(s.GetService<ParseInteractor>(), s.GetService<IEditorService>(), s.GetService<ILogger<FormInteractor>>().Adapt());
            });
            serviceProvider.AddSingleton<ClaimInteractor>();
            return serviceProvider;
        }
    }
}
