using Xunit;

namespace AdvancedSharpAdbClient.Exceptions.Tests
{
    public class DeviceNotFoundExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor() =>
            ExceptionTester<DeviceNotFoundException>.TestEmptyConstructor(() => new DeviceNotFoundException());

        [Fact]
        public void TestMessageAndInnerConstructor() =>
            ExceptionTester<DeviceNotFoundException>.TestMessageAndInnerConstructor((message, inner) => new DeviceNotFoundException(message, inner));

        [Fact]
        public void TestSerializationConstructor() =>
            ExceptionTester<DeviceNotFoundException>.TestSerializationConstructor((info, context) => new DeviceNotFoundException(info, context));
    }
}
