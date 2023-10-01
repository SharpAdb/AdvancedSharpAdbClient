using System;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient.Logs.Tests
{
    public class DummyLogger(string name) : ILogger
    {
        public string Name { get; } = name;

        public List<LogMessage> LogMessages { get; } = new();

        public void Log(LogLevel logLevel, Exception exception, string message, params object[] args) =>
            LogMessages.Add(new LogMessage(logLevel, exception, message, args));
    }

    public class DummyLogger<T> : DummyLogger, ILogger<T>
    {
        public Type Type { get; } = typeof(T);

        public DummyLogger() : base(typeof(T).Name) { }
    }

    public record class LogMessage(LogLevel LogLevel, Exception Exception, string Message, params object[] Args)
    {
        public bool HasFormat => Args.Length > 0;
        public bool HasException => Exception != null;
        public string FormattedMessage => HasFormat ? string.Format(Message, Args) : Message;
    }
}
