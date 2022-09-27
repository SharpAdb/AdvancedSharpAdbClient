using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="DateTimeHelper"/> class.
    /// </summary>
    public class DateTimeHelperTests
    {
        [Fact]
        public void ToUnixEpochTest()
        {
            DateTime time = new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc);
            Assert.Equal(1654085434, time.ToUnixEpoch());
        }

        [Fact]
        public void ToDateTime()
        {
            DateTime time = new DateTime(2022, 6, 1, 12, 10, 34, DateTimeKind.Utc);
            Assert.Equal(time, ((long)1654085434).ToDateTime());
        }
    }
}
