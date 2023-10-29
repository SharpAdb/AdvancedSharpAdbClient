using System;
using Xunit;

namespace AdvancedSharpAdbClient.Polyfills.Tests
{
    /// <summary>
    /// Tests the <see cref="DateTimeExtensions"/> class.
    /// </summary>
    public class DateTimeExtensionsTests
    {
        /// <summary>
        /// Tests the <see cref="DateTimeExtensions.FromUnixTimeSeconds(long)"/> method.
        /// </summary>
        [Fact]
        public void FromUnixTimeSecondsTest()
        {
            DateTimeOffset time = new(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(time, DateTimeExtensions.FromUnixTimeSeconds(1654085434));
        }

#if NETFRAMEWORK && !NET46_OR_GREATER
        /// <summary>
        /// Tests the <see cref="DateTimeExtensions.ToUnixTimeSeconds(DateTimeOffset)"/> method.
        /// </summary>
        [Fact]
        public void ToUnixTimeSecondsTest()
        {
            DateTimeOffset time = new(new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc));
            Assert.Equal(1654085434, time.ToUnixTimeSeconds());
        }
#endif

        /// <summary>
        /// Tests the <see cref="DateTimeExtensions.FromUnixEpoch(long)"/> method.
        /// </summary>
        [Fact]
        public void FromUnixEpochTest()
        {
            DateTime time = new(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc);
            Assert.Equal(time, DateTimeExtensions.FromUnixEpoch(1654085434));
        }

        /// <summary>
        /// Tests the <see cref="DateTimeExtensions.ToUnixEpoch(DateTime)"/> method.
        /// </summary>
        [Fact]
        public void ToUnixEpochTest()
        {
            DateTime time = new(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc);
            Assert.Equal(1654085434, time.ToUnixEpoch());
        }
    }
}
