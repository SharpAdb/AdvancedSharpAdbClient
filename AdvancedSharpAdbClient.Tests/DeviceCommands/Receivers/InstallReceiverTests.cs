using AdvancedSharpAdbClient.DeviceCommands;
using Xunit;

namespace AdvancedSharpAdbClient.Tests.DeviceCommands
{
    public class InstallReceiverTests
    {
        [Fact]
        public void ProcessFailureTest()
        {
            InstallReceiver receiver = new();
            receiver.AddOutput("Failure [message]");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal("message", receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessFailureEmptyMessageTest()
        {
            InstallReceiver receiver = new();
            receiver.AddOutput("Failure [  ]");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallReceiver.UnknownError, receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessFailureNoMessageTest()
        {
            InstallReceiver receiver = new();
            receiver.AddOutput("Failure");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallReceiver.UnknownError, receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessSuccessTest()
        {
            InstallReceiver receiver = new();
            receiver.AddOutput("Success");
            receiver.Flush();

            Assert.True(receiver.Success);
        }
    }
}
