using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="Utilities"/> class.
    /// </summary>
    public class UtilitiesTests
    {
        [Fact]
        public void TryParseTest()
        {
            Assert.True(Utilities.TryParse("BootLoader", false, out DeviceState result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.True(Utilities.TryParse("Bootloader", true, out result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.False(Utilities.TryParse<DeviceState>("Bootloader", false, out _));
            Assert.False(Utilities.TryParse<DeviceState>("Reset", true, out _));
        }

        [Fact]
        public void IsNullOrWhiteSpaceTest()
        {
            Assert.True(" ".IsNullOrWhiteSpace());
            Assert.False(" test ".IsNullOrWhiteSpace());
        }

        [Fact]
        public void JoinTest() =>
            Assert.Equal("Hello World!", Utilities.Join(" ", ["Hello", "World!"]));

        [Fact]
        public void FromUnixTimeSecondsTest()
        {
            DateTimeOffset time = new(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(time, Utilities.FromUnixTimeSeconds(1654085434));
        }

        [Fact]
        public void ToUnixTimeSecondsTest()
        {
            DateTimeOffset time = new(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(1654085434, time.ToUnixTimeSeconds());
        }
    }
}
