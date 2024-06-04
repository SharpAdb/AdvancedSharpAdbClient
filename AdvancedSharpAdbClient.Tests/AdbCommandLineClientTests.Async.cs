using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbCommandLineClientTests
    {
        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersionAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetVersionAsyncTest()
        {
            Version adbVersion = new(1, 0, 41);
            string fileVersion = "34.0.4-android-tools";
            string filePath = "/data/data/com.termux/files/usr/bin/adb";
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new AdbCommandLineStatus(adbVersion, fileVersion, filePath)
            };
            AdbCommandLineStatus status = await commandLine.GetVersionAsync();
            Assert.Equal(adbVersion, status.AdbVersion);
            Assert.Equal(fileVersion, status.FileVersion);
            Assert.Equal(filePath, status.FilePath);
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersionAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetVersionAsyncNullTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = default
            };
            _ = await Assert.ThrowsAsync<AdbException>(() => commandLine.GetVersionAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.GetVersionAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetOutdatedVersionAsyncTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.1"])
            };
            _ = await Assert.ThrowsAsync<AdbException>(() => commandLine.GetVersionAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClient.StartServerAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StartServerAsyncTest()
        {
            DummyAdbCommandLineClient commandLine = new();
            Assert.False(commandLine.ServerStarted);
            await commandLine.StartServerAsync();
            Assert.True(commandLine.ServerStarted);
        }
    }
}
