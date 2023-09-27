using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="Extensions"/> class.
    /// </summary>
    public class ExtensionsTests
    {
        [Fact]
        public void TryParseTest()
        {
            Assert.True(Extensions.TryParse("BootLoader", false, out DeviceState result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.True(Extensions.TryParse("Bootloader", true, out result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.False(Extensions.TryParse<DeviceState>("Bootloader", false, out _));
            Assert.False(Extensions.TryParse<DeviceState>("Reset", true, out _));
        }

        [Fact]
        public void IsNullOrWhiteSpaceTest()
        {
            Assert.True(" ".IsNullOrWhiteSpace());
            Assert.False(" test ".IsNullOrWhiteSpace());
        }

        [Fact]
        public void JoinTest() =>
            Assert.Equal("Hello World!", Extensions.Join(" ", ["Hello", "World!"]));

        [Fact]
        public void FromUnixTimeSecondsTest()
        {
            DateTimeOffset time = new(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(time, Extensions.FromUnixTimeSeconds(1654085434));
        }

        [Fact]
        public void ToUnixTimeSecondsTest()
        {
            DateTimeOffset time = new(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(1654085434, time.ToUnixTimeSeconds());
        }
    }
}
