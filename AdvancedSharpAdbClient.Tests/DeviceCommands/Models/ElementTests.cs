using NSubstitute;
using System;
using System.Drawing;
using System.Linq;
using System.Xml;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="Element"/> class.
    /// </summary>
    public class ElementTests
    {
        public ElementTests()
        {
            XmlDocument doc = new();
            doc.Load(@"Assets/DumpScreen.Clean.xml");
            XmlNode = doc.SelectSingleNode("hierarchy/node");
        }

        private XmlNode XmlNode { get; init; }

        [Fact]
        public void PropertiesTest()
        {
            IAdbClient client = Substitute.For<IAdbClient>();
            DeviceData device = new();

            Element element = Element.FromXmlNode(client, device, XmlNode);
            Assert.Equal(client, element.Client);
            Assert.Equal(device, element.Device);
            Assert.Equal("android.widget.FrameLayout", element.Class);
            Assert.Equal("com.bilibili.app.in", element.Package);
            Assert.Equal(Rectangle.FromLTRB(0, 0, 1440, 3060), element.Bounds);
            Assert.Equal(144, element.GetChildCount());

            Element child = element[0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal(client, child.Client);
            Assert.Equal(device, child.Device);
            Assert.Equal("where-where", child.Text);
            Assert.Equal("android.widget.TextView", child.Class);
            Assert.Equal("com.bilibili.app.in", child.Package);
            Assert.Equal("com.bilibili.app.in:id/header_info_name", child.ResourceID);
            Assert.Equal(Rectangle.FromLTRB(45, 889, 427, 973), child.Bounds);
            Assert.Equal(child, element.FindDescendantOrSelf(x => x.Text == "where-where"));
            Assert.Equal(2, element.FindDescendants().Where(x => x.Text == "where-where").Count());
        }

        [Fact]
        public void EqualityTest()
        {
            IAdbClient client = Substitute.For<IAdbClient>();
            DeviceData device = new();

            Element e1 = Element.FromXmlNode(client, device, XmlNode);
            Element e2 = Element.FromXmlNode(client, device, XmlNode);

            Assert.True(e1 == e2);
            Assert.True(e1.Equals(e2));
            Assert.True(e1.Equals((object)e2));
            Assert.Equal(e1.GetHashCode(), e2.GetHashCode());
            Assert.Equal(e1.FindDescendants(), e2.FindDescendants());
        }
    }
}
