using AdvancedSharpAdbClient.Exceptions;
using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbCommandLineClientTests
    {
        [Fact]
        public async void GetVersionAsyncTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new Version(1, 0, 32)
            };
            Assert.Equal(new Version(1, 0, 32), await commandLine.GetVersionAsync());
        }

        [Fact]
        public async void GetVersionAsyncNullTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = null
            };
            _ = await Assert.ThrowsAsync<AdbException>(() => commandLine.GetVersionAsync());
        }

        [Fact]
        public async void GetOutdatedVersionAsyncTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new Version(1, 0, 1)
            };
            _ = await Assert.ThrowsAsync<AdbException>(() => commandLine.GetVersionAsync());
        }

        [Fact]
        public async void StartServerAsyncTest()
        {
            DummyAdbCommandLineClient commandLine = new();
            Assert.False(commandLine.ServerStarted);
            await commandLine.StartServerAsync();
            Assert.True(commandLine.ServerStarted);
        }
    }
}
