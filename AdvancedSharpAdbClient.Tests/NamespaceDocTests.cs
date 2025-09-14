using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="NamespaceDoc"/> class.
    /// </summary>
    public class NamespaceDocTests
    {
        /// <summary>
        /// Tests the <see cref="NamespaceDoc.Name"/> property.
        /// </summary>
        [Fact]
        public void NameTest()
        {
            Assert.Equal("AdvancedSharpAdbClient", NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.DeviceCommands", DeviceCommands.NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.DeviceCommands.Models", DeviceCommands.Models.NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.DeviceCommands.Receivers", DeviceCommands.Receivers.NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.Exceptions", Exceptions.NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.Logs", Logs.NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.Models", Models.NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.Polyfills", Polyfills.NamespaceDoc.Name);
            Assert.Equal("AdvancedSharpAdbClient.Receivers", Receivers.NamespaceDoc.Name);
        }
    }
}
