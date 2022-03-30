using Microsoft.Extensions.Configuration;
using DocAggregator.API.Core;

namespace DocAggregator.API.Presentation
{
    /// <summary>
    /// ���������� <see cref="IOptionsFactory"/>.
    /// </summary>
    public class OptionsFactoryAdapter : IOptionsFactory
    {
        IConfiguration _configuration;

        public OptionsFactoryAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// ��������� ��������� ������ ����� ������������ � ��������� ���������.
        /// </summary>
        /// <typeparam name="TOptions">���, ���������� ��������� ������ ������������.</typeparam>
        /// <returns>������ ���������� ���� ������, ����������� �� ������������.</returns>
        public TOptions GetOptionsOf<TOptions>() where TOptions : IOptions, new()
        {
            return _configuration.GetSection(new TOptions().GetSection()).Get<TOptions>();
        }
    }
}