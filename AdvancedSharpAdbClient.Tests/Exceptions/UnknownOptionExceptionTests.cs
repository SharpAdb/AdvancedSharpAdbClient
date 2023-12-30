using System;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class UnknownOptionExceptionTests
    {
        [Fact]
        public void EmptyConstructorTest() =>
            ExceptionTester<UnknownOptionException>.EmptyConstructorTest(() => new UnknownOptionException());

        [Fact]
        public void MessageConstructorTest() =>
            ExceptionTester<UnknownOptionException>.MessageConstructorTest(message => new UnknownOptionException(message));

        [Fact]
        public void MessageAndInnerConstructorTest() =>
            ExceptionTester<UnknownOptionException>.MessageAndInnerConstructorTest((message, inner) => new UnknownOptionException(message, inner));

        [Fact]
        [Obsolete]
        public void SerializationConstructorTest() =>
            ExceptionTester<UnknownOptionException>.SerializationConstructorTest((info, context) => new UnknownOptionException(info, context));
    }
}
