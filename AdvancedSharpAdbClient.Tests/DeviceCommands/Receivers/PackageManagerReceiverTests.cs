using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers.Tests
{
    public class PackageManagerReceiverTests
    {
        [Fact]
        public void ParseThirdPartyPackage()
        {
            // Arrange
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            DummyAdbClient client = new();

            PackageManager manager = new(client, device, syncServiceFactory: null, skipInit: true);
            PackageManagerReceiver receiver = new(manager);

            // Act
            receiver.AddOutput("package:/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk=com.google.android.apps.plus");
            receiver.AddOutput("package:/system/app/LegacyCamera.apk=com.android.camera");
            receiver.AddOutput("package:mwc2015.be");
            receiver.Flush();

            // Assert
            Assert.Equal(3, manager.Packages.Count);
            Assert.True(manager.Packages.ContainsKey("com.google.android.apps.plus"));
            Assert.True(manager.Packages.ContainsKey("com.android.camera"));
            Assert.True(manager.Packages.ContainsKey("mwc2015.be"));

            Assert.Equal("/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk", manager.Packages["com.google.android.apps.plus"]);
        }
    }
}
