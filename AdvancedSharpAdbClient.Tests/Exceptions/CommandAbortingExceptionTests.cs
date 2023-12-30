using System;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class CommandAbortingExceptionTests
    {
        [Fact]
        public void EmptyConstructorTest() =>
            ExceptionTester<CommandAbortingException>.EmptyConstructorTest(() => new CommandAbortingException());

        [Fact]
        public void MessageConstructorTest() =>
            ExceptionTester<CommandAbortingException>.MessageConstructorTest(message => new CommandAbortingException(message));

        [Fact]
        public void MessageAndInnerConstructorTest() =>
            ExceptionTester<CommandAbortingException>.MessageAndInnerConstructorTest((message, inner) => new CommandAbortingException(message, inner));

        [Fact]
        [Obsolete]
        public void SerializationConstructorTest() =>
            ExceptionTester<CommandAbortingException>.SerializationConstructorTest((info, context) => new CommandAbortingException(info, context));
    }
}
