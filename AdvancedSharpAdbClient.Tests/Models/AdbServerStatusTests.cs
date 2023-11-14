using System;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbServerStatus"/> class.
    /// </summary>
    public class AdbServerStatusTests
    {
        [Fact]
        public void ToStringTest()
        {
            AdbServerStatus status = new(true, new Version(1, 0, 32));
            Assert.Equal("Version 1.0.32 of the adb daemon is running.", status.ToString());
            status = status with { IsRunning = false };
            Assert.Equal("The adb daemon is not running.", status.ToString());
        }
    }
}
