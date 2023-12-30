using System;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class AdbExceptionTests
    {
        [Fact]
        public void EmptyConstructorTest() =>
            ExceptionTester<AdbException>.EmptyConstructorTest(() => new AdbException());

        [Fact]
        public void MessageConstructorTest() =>
            ExceptionTester<AdbException>.MessageConstructorTest(message => new AdbException(message));

        [Fact]
        public void MessageAndInnerConstructorTest() =>
            ExceptionTester<AdbException>.MessageAndInnerConstructorTest((message, inner) => new AdbException(message, inner));

        [Fact]
        [Obsolete]
        public void SerializationConstructorTest() =>
            ExceptionTester<AdbException>.SerializationConstructorTest((info, context) => new AdbException(info, context));
    }
}
