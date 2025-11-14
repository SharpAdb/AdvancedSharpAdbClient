using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="ForwardData"/> class.
    /// </summary>
    public class ForwardDataTests
    {
        [Fact]
        public void SpecTest()
        {
            ForwardData data = new(
                serialNumber: "emulator-5554",
                local: "tcp:1234",
                remote: "tcp:4321"
            );

            Assert.Equal("tcp:1234", data.LocalSpec.ToString());
            Assert.Equal("tcp:4321", data.RemoteSpec.ToString());
            Assert.Equal("emulator-5554 tcp:1234 tcp:4321", data.ToString());
        }
    }
}
