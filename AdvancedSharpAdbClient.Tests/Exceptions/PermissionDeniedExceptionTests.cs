using System;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class PermissionDeniedExceptionTests
    {
        [Fact]
        public void EmptyConstructorTest() =>
            ExceptionTester<PermissionDeniedException>.EmptyConstructorTest(() => new PermissionDeniedException());

        [Fact]
        public void MessageConstructorTest() =>
            ExceptionTester<PermissionDeniedException>.MessageConstructorTest(message => new PermissionDeniedException(message));

        [Fact]
        public void MessageAndInnerConstructorTest() =>
            ExceptionTester<PermissionDeniedException>.MessageAndInnerConstructorTest((message, inner) => new PermissionDeniedException(message, inner));

        [Fact]
        [Obsolete]
        public void SerializationConstructorTest() =>
            ExceptionTester<PermissionDeniedException>.SerializationConstructorTest((info, context) => new PermissionDeniedException(info, context));
    }
}
