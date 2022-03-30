using Microsoft.Extensions.Logging;
using System;
using IExternalLogger = Microsoft.Extensions.Logging.ILogger;
using IExternalFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IInternalLogger = DocAggregator.API.Core.ILogger;
using IInternalFactory = DocAggregator.API.Core.ILoggerFactory;

namespace DocAggregator.API.Presentation
{
    /// <summary>
    /// Реализация <see cref="Core.ILoggerFactory"/>, позволяющая использовать внутреннюю реализацию логгера.
    /// </summary>
    public class LoggerFactoryAdapter : IInternalFactory
    {
        private IExternalFactory _factory;

        public LoggerFactoryAdapter(IExternalFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Разрешает логгер из стандартной библиотеки во внутренний.
        /// </summary>
        /// <returns>Логгер встроенного интерфейса.</returns>
        public IInternalLogger GetLoggerFor<TCaller>()
        {
            return new Logger(_factory.CreateLogger<TCaller>());
        }
    }

    /// <summary>
    /// Реализация <see cref="Core.ILogger"/> как прокси, использующий <see cref="Microsoft.Extensions.Logging.ILogger"/>.
    /// </summary>
    internal class Logger : IInternalLogger
    {
        IExternalLogger _logger;

        public Logger(IExternalLogger logger)
        {
            _logger = logger;
        }

        public void Trace(string message, params string[] args)
            => _logger.LogTrace(message, args);

        public void Debug(string message, params string[] args)
            => _logger.LogDebug(message, args);

        public void Information(string message, params string[] args)
            => _logger.LogInformation(message, args);

        public void Warning(string message, params string[] args)
            => _logger.LogWarning(message, args);

        public void Warning(Exception exception, string message, params string[] args)
            => _logger.LogWarning(exception, message, args);

        public void Error(string message, params string[] args)
            => _logger.LogError(message, args);

        public void Error(Exception exception, string message, params string[] args)
            => _logger.LogError(exception, message, args);

        public void Critical(string message, params string[] args)
            => _logger.LogCritical(message, args);

        public void Critical(Exception exception, string message, params string[] args)
            => _logger.LogCritical(exception, message, args);
    }
}