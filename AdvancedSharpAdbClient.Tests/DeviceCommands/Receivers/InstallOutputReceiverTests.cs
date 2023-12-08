using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers.Tests
{
    public class InstallOutputReceiverTests
    {
        [Fact]
        public void ProcessErrorTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Error: message");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal("message", receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessErrorEmptyMessageTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Error:");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallOutputReceiver.UnknownError, receiver.ErrorMessage);
        }

        [Fact]
        public void ProcessErrorNoMessageTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Error");
            receiver.Flush();

            Assert.False(receiver.Success);
            Assert.Equal(InstallOutputReceiver.UnknownError, receiver.ErrorMessage);
        }

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
            receiver.AddOutput("Success: message");
            receiver.Flush();

            Assert.True(receiver.Success);
            Assert.Equal("message", receiver.SuccessMessage);
        }

        [Fact]
        public void ProcessSuccessEmptyMessageTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Success:");
            receiver.Flush();

            Assert.True(receiver.Success);
            Assert.Equal(string.Empty, receiver.SuccessMessage);
        }

        [Fact]
        public void ProcessSuccessNoMessageTest()
        {
            InstallOutputReceiver receiver = new();
            receiver.AddOutput("Success");
            receiver.Flush();

            Assert.True(receiver.Success);
            Assert.Equal(string.Empty, receiver.SuccessMessage);
        }
    }
}
