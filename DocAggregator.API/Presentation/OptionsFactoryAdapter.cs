using Microsoft.Extensions.Configuration;
using DocAggregator.API.Core;

namespace DocAggregator.API.Presentation
{
    /// <summary>
    /// Реализация <see cref="IOptionsFactory"/>.
    /// </summary>
    public class OptionsFactoryAdapter : IOptionsFactory
    {
        IConfiguration _configuration;

        public OptionsFactoryAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Позволяет считывать секцию файла конфигурации в выбранный контейнер.
        /// </summary>
        /// <typeparam name="TOptions">Тип, отражающий структуру секции конфигурации.</typeparam>
        /// <returns>Объект выбранного типа секции, заполненный из конфигурации.</returns>
        public TOptions GetOptionsOf<TOptions>() where TOptions : IOptions, new()
        {
            return _configuration.GetSection(new TOptions().GetSection()).Get<TOptions>();
        }
    }
}