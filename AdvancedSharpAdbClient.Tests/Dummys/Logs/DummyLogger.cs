using System;
using System.Diagnostics;

namespace AdvancedSharpAdbClient.Logs.Tests
{
    internal class DummyLogger(string name) : ILogger
    {
        public string Name { get; } = name;

        public void Log(LogLevel logLevel, Exception exception, string message, params object[] args)
        {
            Debug.Write($"{Name} logged: ");
            Debug.WriteLine(new LogMessage(logLevel, exception, message, args));
        }
    }

    internal class DummyLogger<T> : DummyLogger, ILogger<T>
    {
        public Type Type { get; } = typeof(T);

        public DummyLogger() : base(typeof(T).Name) { }
    }

    file record class LogMessage(LogLevel LogLevel, Exception Exception, string Message, params object[] Args)
    {
        public bool HasFormat => Args.Length > 0;
        public bool HasException => Exception != null;
        public string FormattedMessage => HasFormat ? string.Format(Message, Args) : Message;
    }
}
