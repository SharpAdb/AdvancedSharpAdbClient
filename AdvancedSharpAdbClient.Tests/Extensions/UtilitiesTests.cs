using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests.Extensions
{
    /// <summary>
    /// Tests the <see cref="Utilities"/> class.
    /// </summary>
    public class UtilitiesTests
    {
        [Fact]
        public void TryParseTest()
        {
            DeviceState result;
            Assert.True(Utilities.TryParse("BootLoader", false, out result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.True(Utilities.TryParse("Bootloader", true, out result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.True(!Utilities.TryParse<DeviceState>("Bootloader", false, out _));
            Assert.True(!Utilities.TryParse<DeviceState>("Reset", true, out _));
        }

        [Fact]
        public void IsNullOrWhiteSpaceTest()
        {
            Assert.True(" ".IsNullOrWhiteSpace());
            Assert.True(!" test ".IsNullOrWhiteSpace());
        }

        [Fact]
        public void FromUnixTimeSecondsTest()
        {
            DateTimeOffset time = new DateTimeOffset(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(time, Utilities.FromUnixTimeSeconds(1654085434));
        }

        [Fact]
        public void ToUnixTimeSecondsTest()
        {
            DateTimeOffset time = new DateTimeOffset(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(1654085434, time.ToUnixTimeSeconds());
        }
    }
}
