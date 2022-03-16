using System;

namespace DocAggregator.API.Core
{
    public interface ILogger
    {
        void Trace(string message, params string[] args);
        void Debug(string message, params string[] args);
        void Information(string message, params string[] args);
        void Warning(string message, params string[] args);
        void Warning(Exception exception, string message, params string[] args);
        void Error(string message, params string[] args);
        void Error(Exception exception, string message, params string[] args);
        void Critical(string message, params string[] args);
        void Critical(Exception exception, string message, params string[] args);
    }
}
