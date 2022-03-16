using Microsoft.Extensions.Logging;
using System;
using IExternalLogger = Microsoft.Extensions.Logging.ILogger;
using IInternalLogger = DocAggregator.API.Core.ILogger;

namespace DocAggregator.API.Presentation
{
    public static class LoggerAdapter
    {
        /// <summary>
        /// Разрешает логгер из стандартной библиотеки во внутренний.
        /// </summary>
        /// <param name="logger">Служба ведения журнала стандартного интерфейса.</param>
        /// <returns>Логгер встроенного интерфейса.</returns>
        public static IInternalLogger Adapt(this IExternalLogger logger)
        {
            return new Logger(logger);
        }

        private class Logger : IInternalLogger
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
}