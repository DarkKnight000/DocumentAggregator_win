using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace DocAggregator.API
{
    /// <summary>
    /// �������� ������ ������� ������������ ��������� ��������.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// ��������� �������-�������� ��� ������ � �������� ���������.
        /// </summary>
        /// <returns>�� �� ��������� ��������, ���������� � ����� ����������.</returns>
        public static IServiceCollection AddAdapters(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactoryAdapter>();
            serviceCollection.AddSingleton<IOptionsFactory, OptionsFactoryAdapter>();
            return serviceCollection;
        }

        /// <summary>
        /// ��������� ������� ����������� ��� ���� ����������.
        /// </summary>
        /// <returns>�� �� ��������� ��������, ���������� � ����� ����������.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
        {
            // ������ ������� ������������, �� ������������� �����.
            serviceCollection.AddSingleton<Infrastructure.OracleManaged.SqlConnectionResource>();
            serviceCollection.AddSingleton<Infrastructure.OracleManaged.TemplateMap>();
            serviceCollection.AddSingleton<Infrastructure.OracleManaged.ModelBind>();
            // ������, ����������� � ������������ �����.
            serviceCollection.AddSingleton<IEditorService, Infrastructure.OpenXMLProcessing.EditorService>();
            serviceCollection.AddSingleton<IClaimRepository, Infrastructure.OracleManaged.ClaimRepository>();
            return serviceCollection;
        }

        /// <summary>
        /// ��������� ����������� ���� ����������.
        /// </summary>
        /// <returns>�� �� ��������� ��������, ���������� � ����� ����������.</returns>
        public static IServiceCollection AddInteractors(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ParseInteractor>();
            serviceCollection.AddSingleton<FormInteractor>();
            serviceCollection.AddSingleton<ClaimInteractor>();
            return serviceCollection;
        }
    }
}
