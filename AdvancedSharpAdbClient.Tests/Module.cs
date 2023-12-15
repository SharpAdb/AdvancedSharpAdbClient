using System.Runtime.CompilerServices;

namespace AdvancedSharpAdbClient.Tests
{
    public static class Module
    {
        [ModuleInitializer]
        public static void ModuleMain() => LoggerProvider.SetLogProvider(new DummyLoggerFactory());
    }
}
