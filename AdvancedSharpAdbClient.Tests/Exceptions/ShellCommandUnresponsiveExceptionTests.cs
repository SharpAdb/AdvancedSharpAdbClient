using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class ShellCommandUnresponsiveExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor() =>
            ExceptionTester<ShellCommandUnresponsiveException>.TestEmptyConstructor(() => new ShellCommandUnresponsiveException());

        [Fact]
        public void TestMessageConstructor() =>
            ExceptionTester<ShellCommandUnresponsiveException>.TestMessageConstructor((message) => new ShellCommandUnresponsiveException(message));

        [Fact]
        public void TestMessageAndInnerConstructor() =>
            ExceptionTester<ShellCommandUnresponsiveException>.TestMessageAndInnerConstructor((message, inner) => new ShellCommandUnresponsiveException(message, inner));

        [Fact]
        public void TestSerializationConstructor() =>
            ExceptionTester<ShellCommandUnresponsiveException>.TestSerializationConstructor((info, context) => new ShellCommandUnresponsiveException(info, context));
    }
}
