using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class PermissionDeniedExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor() =>
            ExceptionTester<PermissionDeniedException>.TestEmptyConstructor(() => new PermissionDeniedException());

        [Fact]
        public void TestMessageConstructor() =>
            ExceptionTester<PermissionDeniedException>.TestMessageConstructor((message) => new PermissionDeniedException(message));

        [Fact]
        public void TestMessageAndInnerConstructor() =>
            ExceptionTester<PermissionDeniedException>.TestMessageAndInnerConstructor((message, inner) => new PermissionDeniedException(message, inner));

        [Fact]
        public void TestSerializationConstructor() =>
            ExceptionTester<PermissionDeniedException>.TestSerializationConstructor((info, context) => new PermissionDeniedException(info, context));
    }
}
