using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class UnknownOptionExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor() =>
            ExceptionTester<UnknownOptionException>.TestEmptyConstructor(() => new UnknownOptionException());

        [Fact]
        public void TestMessageConstructor() =>
            ExceptionTester<UnknownOptionException>.TestMessageConstructor((message) => new UnknownOptionException(message));

        [Fact]
        public void TestMessageAndInnerConstructor() =>
            ExceptionTester<UnknownOptionException>.TestMessageAndInnerConstructor((message, inner) => new UnknownOptionException(message, inner));

        [Fact]
        public void TestSerializationConstructor() =>
            ExceptionTester<UnknownOptionException>.TestSerializationConstructor((info, context) => new UnknownOptionException(info, context));
    }
}
