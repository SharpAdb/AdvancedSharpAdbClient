using System;
using System.IO;
using Xunit;

namespace AdvancedSharpAdbClient.Receivers.Tests
{
    /// <summary>
    /// Tests the <see cref="ConsoleOutputReceiver"/> class.
    /// </summary>
    public class ConsoleOutputReceiverTests
    {
        [Fact]
        public void ToStringTest()
        {
            ConsoleOutputReceiver receiver = new();
            receiver.AddOutput("Hello, World!");
            receiver.AddOutput("See you!");

            receiver.Flush();

            Assert.Equal("Hello, World!\r\nSee you!\r\n",
                receiver.ToString(),
                ignoreLineEndingDifferences: true);
        }

        [Theory]
        [InlineData("#Hello, World!", "See you!", "See you!\r\n")]
        [InlineData("Hello, World!", "$See you!", "Hello, World!\r\n")]
        public void ToStringIgnoredLineTest(string line1, string line2, string result)
        {
            ConsoleOutputReceiver receiver = new();
            receiver.AddOutput(line1);
            receiver.AddOutput(line2);

            receiver.Flush();

            Assert.Equal(result,
                receiver.ToString(),
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ThrowOnErrorTest()
        {
            AssertThrowsException<FileNotFoundException>("/dev/test: not found");
            AssertThrowsException<FileNotFoundException>("No such file or directory");
            AssertThrowsException<UnknownOptionException>("Unknown option -h");
            AssertThrowsException<CommandAbortingException>("/dev/test: Aborting.");
            AssertThrowsException<FileNotFoundException>("/dev/test: applet not found");
            AssertThrowsException<PermissionDeniedException>("/dev/test: permission denied");
            AssertThrowsException<PermissionDeniedException>("/dev/test: access denied");

            // Should not thrown an exception
            ConsoleOutputReceiver receiver = new() { ParsesErrors = true };
            receiver.AddOutput("Stay calm and watch cat movies.");
        }

        private static void AssertThrowsException<T>(string line) where T : Exception
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = true };
            _ = Assert.Throws<T>(() => receiver.AddOutput(line));
        }
    }
}
