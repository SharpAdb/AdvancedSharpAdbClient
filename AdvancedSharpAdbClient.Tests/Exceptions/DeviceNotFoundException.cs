using System;
using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class DeviceNotFoundExceptionTests
    {
        [Fact]
        public void EmptyConstructorTest() =>
            ExceptionTester<DeviceNotFoundException>.EmptyConstructorTest(() => new DeviceNotFoundException());

        [Fact]
        public void MessageAndInnerConstructorTest() =>
            ExceptionTester<DeviceNotFoundException>.MessageAndInnerConstructorTest((message, inner) => new DeviceNotFoundException(message, inner));

        [Fact]
        [Obsolete]
        public void SerializationConstructorTest() =>
            ExceptionTester<DeviceNotFoundException>.SerializationConstructorTest((info, context) => new DeviceNotFoundException(info, context));
    }
}
