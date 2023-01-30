using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbResponse"/> class.
    /// </summary>
    public class AdbResponseTests
    {
        [Fact]
        public void EqualsTest()
        {
            AdbResponse first = new()
            {
                IOSuccess = false,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            AdbResponse second = new()
            {
                IOSuccess = true,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            Assert.False(first.Equals("some string"));
            Assert.False(first.Equals(second));
            Assert.True(first.Equals(first));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            AdbResponse first = new()
            {
                IOSuccess = false,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            AdbResponse second = new()
            {
                IOSuccess = false,
                Message = "Hi",
                Okay = false,
                Timeout = false
            };

            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void ToStringTest()
        {
            Assert.Equal("OK", AdbResponse.OK.ToString());
            Assert.Equal("Error: Huh?", AdbResponse.FromError("Huh?").ToString());
        }
    }
}
