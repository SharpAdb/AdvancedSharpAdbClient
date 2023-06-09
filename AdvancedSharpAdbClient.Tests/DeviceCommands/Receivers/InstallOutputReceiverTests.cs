using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    public class InstallOutputReceiverTests
    {
        [Fact]
        public void ProcessFailureTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Failure [message]");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal("message", receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessFailureEmptyMessageTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Failure [  ]");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallOutputReceiver.UnknownError, receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessFailureNoMessageTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Failure");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallOutputReceiver.UnknownError, receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessSuccessTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Success");
            receiver.Flush();

            Assert.True(receiver.Success);
        }
    }
}
