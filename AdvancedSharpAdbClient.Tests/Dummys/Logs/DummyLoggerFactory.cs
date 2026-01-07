namespace AdvancedSharpAdbClient.Logs.Tests
{
    internal class DummyLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName) => new DummyLogger(categoryName);

        public ILogger<T> CreateLogger<T>() => new DummyLogger<T>();
    }
}
