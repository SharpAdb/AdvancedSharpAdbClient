namespace AdvancedSharpAdbClient.Logs.Tests
{
    public class DummyLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName) => new DummyLogger(categoryName);

        public ILogger<T> CreateLogger<T>() => new DummyLogger<T>();
    }
}
