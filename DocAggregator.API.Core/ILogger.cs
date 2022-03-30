using System;

namespace DocAggregator.API.Core
{
    /// <summary>
    /// Предоставляет соглашения для внутреннего ведения журнала.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Записать в журнал сообщение об отладочной информации.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Debug(string message, params string[] args);

        /// <summary>
        /// Записать в журнал сообщение о подробном ходе выполнения.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Trace(string message, params string[] args);

        /// <summary>
        /// Записать в журнал сообщение об этапах выполнения.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Information(string message, params string[] args);

        /// <summary>
        /// Записать в журнал предупреждение.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Warning(string message, params string[] args);

        /// <summary>
        /// Записать в журнал предупреждение с соответствующим исключением.
        /// </summary>
        /// <param name="exception">Связанное с записью исключение.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Warning(Exception exception, string message, params string[] args);

        /// <summary>
        /// Записать в журнал некритическую ошибку.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Error(string message, params string[] args);

        /// <summary>
        /// Записать в журнал некритическую ошибку с соответствующим исключением.
        /// </summary>
        /// <param name="exception">Связанное с записью исключение.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Error(Exception exception, string message, params string[] args);

        /// <summary>
        /// Записать в журнал критическую ошибку.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Critical(string message, params string[] args);

        /// <summary>
        /// Записать в журнал критическую ошибку с соответствующим исключением.
        /// </summary>
        /// <param name="exception">Связанное с записью исключение.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Аргументы форматирования.</param>
        void Critical(Exception exception, string message, params string[] args);
    }
}
