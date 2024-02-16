using System;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="VersionInfo"/> class.
    /// </summary>
    public class VersionInfoTests
    {
        /// <summary>
        /// Tests the <see cref="VersionInfo.TryAsVersion(out Version?)"/> method.
        /// </summary>
        [Theory]
        [InlineData(1231, "1.2.3.1", true)]
        [InlineData(9393, "v9.3.9.3", true)]
        [InlineData(9393, "9.3.9.3.9.3", true)]
        [InlineData(3450, "One.3.Two.4.5", true)]
        [InlineData(12345432, "Version", false)]
        [InlineData(098765456, "Unknown", false)]
        public void TryAsVersionTest(int versionCode, string versionName, bool expected)
        {
            bool result = new VersionInfo(versionCode, versionName).TryAsVersion(out Version version);
            Assert.Equal(expected, result);
            if (expected)
            {
                Assert.Equal(versionCode, (version.Major * 1000) + (version.Minor * 100) + (version.Build * 10) + version.Revision);
            }
            else
            {
                Assert.Null(version);
            }
        }

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Tests the <see cref="VersionInfo.TryAsPackageVersion(out PackageVersion)"/> method.
        /// </summary>
        [Theory]
        [InlineData(1231, "1.2.3.1", true)]
        [InlineData(9393, "v9.3.9.3", true)]
        [InlineData(9393, "9.3.9.3.9.3", true)]
        [InlineData(3450, "One.3.Two.4.5", true)]
        [InlineData(12345432, "Version", false)]
        [InlineData(098765456, "Unknown", false)]
        public void TryAsPackageVersionTest(int versionCode, string versionName, bool expected)
        {
            bool result = new VersionInfo(versionCode, versionName).TryAsPackageVersion(out PackageVersion version);
            Assert.Equal(expected, result);
            if (expected)
            {
                Assert.Equal(versionCode, (version.Major * 1000) + (version.Minor * 100) + (version.Build * 10) + version.Revision);
            }
            else
            {
                Assert.Equal(default, version);
            }
        }
#endif

        /// <summary>
        /// Tests the <see cref="VersionInfo.Deconstruct(out int, out string)"/> method.
        /// </summary>
        [Theory]
        [InlineData(1231, "1.2.3.1")]
        [InlineData(9393, "v9.3.9.3")]
        [InlineData(12345432, "Version")]
        [InlineData(098765456, "Unknown")]
        public void DeconstructTest(int versionCode, string versionName)
        {
            VersionInfo version = new(versionCode, versionName);
            (int code, string name) = version;
            Assert.Equal(versionCode, code);
            Assert.Equal(versionName, name);
        }

        /// <summary>
        /// Tests the <see cref="VersionInfo.ToString"/> method.
        /// </summary>
        [Fact]
        public void ToStringTest()
        {
            VersionInfo version = new(1234, "1.2.3.4");
            Assert.Equal("1.2.3.4 (1234)", version.ToString());
        }
    }
}
