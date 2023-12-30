using System;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class ShellCommandUnresponsiveExceptionTests
    {
        [Fact]
        public void EmptyConstructorTest() =>
            ExceptionTester<ShellCommandUnresponsiveException>.EmptyConstructorTest(() => new ShellCommandUnresponsiveException());

        [Fact]
        public void MessageConstructorTest() =>
            ExceptionTester<ShellCommandUnresponsiveException>.MessageConstructorTest(message => new ShellCommandUnresponsiveException(message));

        [Fact]
        public void MessageAndInnerConstructorTest() =>
            ExceptionTester<ShellCommandUnresponsiveException>.MessageAndInnerConstructorTest((message, inner) => new ShellCommandUnresponsiveException(message, inner));

        [Fact]
        [Obsolete]
        public void SerializationConstructorTest() =>
            ExceptionTester<ShellCommandUnresponsiveException>.SerializationConstructorTest((info, context) => new ShellCommandUnresponsiveException(info, context));
    }
}
