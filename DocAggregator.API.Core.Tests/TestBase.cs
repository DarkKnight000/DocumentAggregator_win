using Moq;
using System;
using System.Diagnostics;

namespace DocAggregator.API.Core.Tests
{
    public abstract class TestBase
    {
        public ILogger Logger { get; private set; }
        public ILoggerFactory LoggerFactory { get; private set; }

        protected TestBase()
        {
            if (Debugger.IsAttached)
            {
                Logger = new TestLogger();
            }
            else
            {
                Logger = Mock.Of<ILogger>();
            }
            LoggerFactory = Mock.Of<ILoggerFactory>(factory => factory.GetLoggerFor<object>() == Logger);
        }

        private class TestLogger : ILogger
        {
            public void Critical(string message, params object[] args) =>
                Debugger.Log(1, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Critical(Exception exception, string message, params object[] args) =>
                Debugger.Log(1, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Debug(string message, params object[] args) =>
                Debugger.Log(6, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Error(string message, params object[] args) =>
                Debugger.Log(2, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Error(Exception exception, string message, params object[] args) =>
                Debugger.Log(2, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Information(string message, params object[] args) =>
                Debugger.Log(4, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Trace(string message, params object[] args) =>
                Debugger.Log(5, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Warning(string message, params object[] args) =>
                Debugger.Log(3, "test logger", string.Format(message, args) + Environment.NewLine);
            public void Warning(Exception exception, string message, params object[] args) =>
                Debugger.Log(3, "test logger", string.Format(message, args) + Environment.NewLine);
        }
    }
}
