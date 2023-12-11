using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="VersionInfo"/> class.
    /// </summary>
    public class VersionInfoTests
    {
        /// <summary>
        /// Tests the <see cref="VersionInfo.Deconstruct(out int, out string)"/> method.
        /// </summary>
        [Theory]
        [InlineData(1231, "1.2.3.1")]
        [InlineData(9393, "9.3.9.3")]
        [InlineData(12345432, "Version")]
        [InlineData(098765456, "Unknown")]
        public void DeconstructTest(int versionCode, string versionName)
        {
            VersionInfo version = new(versionCode, versionName);
            (int code, string name) = version;
            Assert.Equal(versionCode, code);
            Assert.Equal(versionName, name);
        }

        [Fact]
        public void ToStringTest()
        {
            VersionInfo version = new(1234, "1.2.3.4");
            Assert.Equal("1.2.3.4 (1234)", version.ToString());
        }
    }
}
