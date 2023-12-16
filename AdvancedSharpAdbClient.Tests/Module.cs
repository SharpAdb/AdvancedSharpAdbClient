using System.Runtime.CompilerServices;

namespace AdvancedSharpAdbClient.Tests
{
    public static class Module
    {
        [ModuleInitializer]
        public static void Initialize() => LoggerProvider.SetLogProvider(new DummyLoggerFactory());
    }
}
