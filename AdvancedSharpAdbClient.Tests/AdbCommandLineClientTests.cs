using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbCommandLineClient"/> class.
    /// </summary>
    public partial class AdbCommandLineClientTests
    {
        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersion"/> method.
        /// </summary>
        [Fact]
        public void GetVersionTest()
        {
            Version adbVersion = new(1, 0, 41);
            string fileVersion = "34.0.4-android-tools";
            string filePath = "/data/data/com.termux/files/usr/bin/adb";
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new AdbCommandLineStatus(adbVersion, fileVersion, filePath)
            };
            AdbCommandLineStatus status = commandLine.GetVersion();
            Assert.Equal(adbVersion, status.AdbVersion);
            Assert.Equal(fileVersion, status.FileVersion);
            Assert.Equal(filePath, status.FilePath);
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersion"/> method.
        /// </summary>
        [Fact]
        public void GetVersionNullTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = default
            };
            _ = Assert.Throws<AdbException>(() => commandLine.GetVersion());
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersion"/> method.
        /// </summary>
        [Fact]
        public void GetOutdatedVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.1"])
            };
            _ = Assert.Throws<AdbException>(() => commandLine.GetVersion());
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.StartServer"/> method.
        /// </summary>
        [Fact]
        public void StartServerTest()
        {
            DummyAdbCommandLineClient commandLine = new();
            Assert.False(commandLine.ServerStarted);
            commandLine.StartServer();
            Assert.True(commandLine.ServerStarted);
        }
    }
}
