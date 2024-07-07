using System;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbCommandLineStatus"/> class.
    /// </summary>
    public class AdbCommandLineStatusTests
    {
        [Fact]
        public void GetVersionFromOutputTest()
        {
            AdbCommandLineStatus status =
                AdbCommandLineStatus.GetVersionFromOutput(
                    [
                        "Android Debug Bridge version 1.0.41",
                        "Version 34.0.4-android-tools",
                        "Installed as /data/data/com.termux/files/usr/bin/adb"
                    ]);
            Assert.Equal(new Version(1, 0, 41), status.AdbVersion);
            Assert.Equal("34.0.4-android-tools", status.FileVersion);
            Assert.Equal("/data/data/com.termux/files/usr/bin/adb", status.FilePath);
        }

        [Fact]
        public void ToStringTest()
        {
            AdbCommandLineStatus status = new(new Version(1, 0, 41), "34.0.4-android-tools", "/data/data/com.termux/files/usr/bin/adb");
            Assert.Equal(
                """
                Android Debug Bridge version 1.0.41
                Version 34.0.4-android-tools
                Installed as /data/data/com.termux/files/usr/bin/adb
                """, status.ToString(), ignoreLineEndingDifferences: true);
        }
    }
}
