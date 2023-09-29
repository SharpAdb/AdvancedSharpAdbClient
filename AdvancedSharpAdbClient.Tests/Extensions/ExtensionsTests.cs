using System.Collections.Generic;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="Extensions"/> class.
    /// </summary>
    public class ExtensionsTests
    {
        /// <summary>
        /// Tests the <see cref="Extensions.TryParse{TEnum}(string, bool, out TEnum)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="Extensions.IsNullOrWhiteSpace(string)"/> method.
        /// </summary>
        [Fact]
        public void IsNullOrWhiteSpaceTest()
        {
            Assert.True(" ".IsNullOrWhiteSpace());
            Assert.False(" test ".IsNullOrWhiteSpace());
        }

        /// <summary>
        /// Tests the <see cref="Extensions.Join(string, IEnumerable{string})"/> method.
        /// </summary>
        [Fact]
        public void JoinTest() =>
            Assert.Equal("Hello World!", Extensions.Join(" ", ["Hello", "World!"]));
    }
}
