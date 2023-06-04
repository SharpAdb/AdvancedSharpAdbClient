using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class CommandAbortingExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor() =>
            ExceptionTester<CommandAbortingException>.TestEmptyConstructor(() => new CommandAbortingException());

        [Fact]
        public void TestMessageConstructor() =>
            ExceptionTester<CommandAbortingException>.TestMessageConstructor((message) => new CommandAbortingException(message));

        [Fact]
        public void TestMessageAndInnerConstructor() =>
            ExceptionTester<CommandAbortingException>.TestMessageAndInnerConstructor((message, inner) => new CommandAbortingException(message, inner));

        [Fact]
        public void TestSerializationConstructor() =>
            ExceptionTester<CommandAbortingException>.TestSerializationConstructor((info, context) => new CommandAbortingException(info, context));
    }
}
