using Microsoft.Extensions.Logging;
using System;
using IExternalLogger = Microsoft.Extensions.Logging.ILogger;
using IExternalFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IInternalLogger = DocAggregator.API.Core.ILogger;
using IInternalFactory = DocAggregator.API.Core.ILoggerFactory;
using System.Collections.Generic;

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
    public class Logger : IInternalLogger
    {
        public const int HISTORY_LENGTH = 200;

        public static IEnumerable<HistoryEntry> GlobalHistory => _history;
        private static int _historyCounter;
        private static LinkedList<HistoryEntry> _history;

        IExternalLogger _logger;

        static Logger()
        {
            _historyCounter = 0;
            _history = new LinkedList<HistoryEntry>();
        }

        public Logger(IExternalLogger logger)
        {
            _logger = logger;
        }

        public void Trace(string message, params object[] args)
        {
            var trace = Environment.StackTrace;
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                StackTrace = trace.Substring(trace.IndexOf('\n', 0, 2) + 1),
                Severity = Severity.Trace
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogTrace(message, args);
        }

        public void Debug(string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                StackTrace = Environment.StackTrace,
                Severity = Severity.Debug
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogDebug(message, args);
        }

        public void Information(string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                StackTrace = Environment.StackTrace,
                Severity = Severity.Information
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogInformation(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                StackTrace = Environment.StackTrace,
                Severity = Severity.Warning
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogWarning(message, args);
        }

        public void Warning(Exception exception, string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                Exception = exception,
                StackTrace = Environment.StackTrace,
                Severity = Severity.Warning
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogWarning(exception, message, args);
        }

        public void Error(string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                StackTrace = Environment.StackTrace,
                Severity = Severity.Error
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogError(message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                Exception = exception,
                StackTrace = Environment.StackTrace,
                Severity = Severity.Error
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogError(exception, message, args);
        }

        public void Critical(string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                StackTrace = Environment.StackTrace,
                Severity = Severity.Critical
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogCritical(message, args);
        }

        public void Critical(Exception exception, string message, params object[] args)
        {
            _history.AddFirst(new HistoryEntry() {
                Time = DateTime.Now,
                Message = string.Format(message, args),
                Exception = exception,
                StackTrace = Environment.StackTrace,
                Severity = Severity.Critical
            });
            if (++_historyCounter > HISTORY_LENGTH) _history.RemoveLast();
            _logger.LogCritical(exception, message, args);
        }
    }

    public enum Severity
    {
        Unknown,
        Critical,
        Error,
        Warning,
        Information,
        Debug,
        Trace
    }

    public struct HistoryEntry
    {
        public Severity Severity { get; set; }
        public string StackTrace { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public DateTime Time { get; set; }
    }
}