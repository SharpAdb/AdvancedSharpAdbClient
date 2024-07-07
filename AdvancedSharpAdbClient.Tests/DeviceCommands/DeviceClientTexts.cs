using NSubstitute;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    public partial class DeviceClientTexts
    {
        protected static DeviceData Device { get; } = new()
        {
            Serial = "169.254.109.177:5555",
            State = DeviceState.Online
        };

        /// <summary>
        /// Tests the <see cref="DeviceClient(IAdbClient, DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void ConstructorNullTest()
        {
            _ = Assert.Throws<ArgumentNullException>(() => new DeviceClient(null, default));
            _ = Assert.Throws<ArgumentNullException>(() => new DeviceClient(null, new DeviceData { Serial = "169.254.109.177:5555" }));
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => new DeviceClient(Substitute.For<IAdbClient>(), new DeviceData()));
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.Deconstruct(out IAdbClient, out DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DeconstructTest()
        {
            IAdbClient client = Substitute.For<IAdbClient>();
            DeviceClient deviceClient = new(client, Device);
            (IAdbClient adbClient, DeviceData device) = deviceClient;
            Assert.Equal(client, adbClient);
            Assert.Equal(Device, device);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.Equals(DeviceClient?)"/> method.
        /// </summary>
        [Fact]
        public void EqualityTest()
        {
            IAdbClient c1 = Substitute.For<IAdbClient>();
            IAdbClient c2 = Substitute.For<IAdbClient>();
            DeviceData d1 = new() { Serial = "1" };
            DeviceData d2 = new() { Serial = "2" };

            DeviceClient p1 = new(c1, d1);
            DeviceClient p2 = new(c2, d2);
            DeviceClient p3 = new(c1, d1);

            Assert.True(p1 == p3);
            Assert.True(p1 != p2);
            Assert.True(p2 != p3);

            Assert.True(p1.Equals(p3));
            Assert.False(p1.Equals(p2));
            Assert.False(p2.Equals(p3));

            Assert.True(p1.Equals((object)p3));
            Assert.False(p1.Equals((object)p2));
            Assert.False(p2.Equals((object)p3));

            Assert.Equal(p1.GetHashCode(), p3.GetHashCode());
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient(DeviceClient)"/> method.
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            IAdbClient client = Substitute.For<IAdbClient>();
            IAdbClient newClient = Substitute.For<IAdbClient>();
            DeviceData device = new() { Serial = "1" };
            DeviceData newDevice = new() { Serial = "2" };

            DeviceClient deviceClient = new(client, device);
            Assert.Equal(client, deviceClient.AdbClient);
            Assert.Equal(device, deviceClient.Device);

            deviceClient = deviceClient with { };
            Assert.Equal(client, deviceClient.AdbClient);
            Assert.Equal(device, deviceClient.Device);

            deviceClient = deviceClient with { AdbClient = newClient };
            Assert.Equal(newClient, deviceClient.AdbClient);
            Assert.Equal(device, deviceClient.Device);

            deviceClient = deviceClient with { Device = newDevice };
            Assert.Equal(newClient, deviceClient.AdbClient);
            Assert.Equal(newDevice, deviceClient.Device);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenString()"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringTest()
        {
            string dump = File.ReadAllText(@"Assets/DumpScreen.txt");
            string cleanDump = File.ReadAllText(@"Assets/DumpScreen.Clean.xml");

            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = dump;

            string xml = new DeviceClient(client, Device).DumpScreenString();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            Assert.Equal(cleanDump, xml);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenString()"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringMIUITest()
        {
            string miuidump = File.ReadAllText(@"Assets/DumpScreen.MIUI.txt");
            string cleanMIUIDump = File.ReadAllText(@"Assets/DumpScreen.Clean.xml");

            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = miuidump;

            string miuiXml = new DeviceClient(client, Device).DumpScreenString();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            Assert.Equal(cleanMIUIDump, miuiXml);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenString()"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringEmptyTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = string.Empty;

            string emptyXml = new DeviceClient(client, Device).DumpScreenString();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            Assert.True(string.IsNullOrEmpty(emptyXml));
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenString()"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringErrorTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.Error.txt");

            _ = Assert.Throws<XmlException>(() => new DeviceClient(client, Device).DumpScreenString());

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreen()"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.txt");

            XmlDocument xml = new DeviceClient(client, Device).DumpScreen();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            string cleanDump = File.ReadAllText(@"Assets/DumpScreen.Clean.xml");
            XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc, xml);
        }

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenWinRT()"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenWinRTTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.txt");

            Windows.Data.Xml.Dom.XmlDocument xml = new DeviceClient(client, Device).DumpScreenWinRT();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            string cleanDump = File.ReadAllText(@"Assets/DumpScreen.Clean.xml");
            Windows.Data.Xml.Dom.XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc.InnerText, xml.InnerText);
        }

#endif

        /// <summary>
        /// Tests the <see cref="DeviceClient.Click(int, int)"/> method.
        /// </summary>
        [Fact]
        public void ClickTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input tap 100 100"] =
                """
                java.lang.SecurityException: Injecting to another application requires INJECT_EVENTS permission
                        at android.os.Parcel.createExceptionOrNull(Parcel.java:2373)
                        at android.os.Parcel.createException(Parcel.java:2357)
                        at android.os.Parcel.readException(Parcel.java:2340)
                        at android.os.Parcel.readException(Parcel.java:2282)
                        at android.hardware.input.IInputManager$Stub$Proxy.injectInputEvent(IInputManager.java:946)
                        at android.hardware.input.InputManager.injectInputEvent(InputManager.java:907)
                        at com.android.commands.input.Input.injectMotionEvent(Input.java:397)
                        at com.android.commands.input.Input.access$200(Input.java:41)
                        at com.android.commands.input.Input$InputTap.sendTap(Input.java:223)
                        at com.android.commands.input.Input$InputTap.run(Input.java:217)
                        at com.android.commands.input.Input.onRun(Input.java:107)
                        at com.android.internal.os.BaseCommand.run(BaseCommand.java:60)
                        at com.android.commands.input.Input.main(Input.java:71)
                        at com.android.internal.os.RuntimeInit.nativeFinishInit(Native Method)
                        at com.android.internal.os.RuntimeInit.main(RuntimeInit.java:438)
                Caused by: android.os.RemoteException: Remote stack trace:
                        at com.android.server.input.InputManagerService.injectInputEventInternal(InputManagerService.java:677)
                        at com.android.server.input.InputManagerService.injectInputEvent(InputManagerService.java:651)
                        at android.hardware.input.IInputManager$Stub.onTransact(IInputManager.java:430)
                        at android.os.Binder.execTransactInternal(Binder.java:1165)
                        at android.os.Binder.execTransact(Binder.java:1134)
                """;

            JavaException exception = Assert.Throws<JavaException>(() => new DeviceClient(client, Device).Click(100, 100));

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input tap 100 100", client.ReceivedCommands[0]);

            Assert.Equal("SecurityException", exception.JavaName);
            Assert.Equal("Injecting to another application requires INJECT_EVENTS permission", exception.Message);
            Assert.Equal(
                """
                        at android.os.Parcel.createExceptionOrNull(Parcel.java:2373)
                        at android.os.Parcel.createException(Parcel.java:2357)
                        at android.os.Parcel.readException(Parcel.java:2340)
                        at android.os.Parcel.readException(Parcel.java:2282)
                        at android.hardware.input.IInputManager$Stub$Proxy.injectInputEvent(IInputManager.java:946)
                        at android.hardware.input.InputManager.injectInputEvent(InputManager.java:907)
                        at com.android.commands.input.Input.injectMotionEvent(Input.java:397)
                        at com.android.commands.input.Input.access$200(Input.java:41)
                        at com.android.commands.input.Input$InputTap.sendTap(Input.java:223)
                        at com.android.commands.input.Input$InputTap.run(Input.java:217)
                        at com.android.commands.input.Input.onRun(Input.java:107)
                        at com.android.internal.os.BaseCommand.run(BaseCommand.java:60)
                        at com.android.commands.input.Input.main(Input.java:71)
                        at com.android.internal.os.RuntimeInit.nativeFinishInit(Native Method)
                        at com.android.internal.os.RuntimeInit.main(RuntimeInit.java:438)
                Caused by: android.os.RemoteException: Remote stack trace:
                        at com.android.server.input.InputManagerService.injectInputEventInternal(InputManagerService.java:677)
                        at com.android.server.input.InputManagerService.injectInputEvent(InputManagerService.java:651)
                        at android.hardware.input.IInputManager$Stub.onTransact(IInputManager.java:430)
                        at android.os.Binder.execTransactInternal(Binder.java:1165)
                        at android.os.Binder.execTransact(Binder.java:1134)
                """, exception.JavaStackTrace, ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.Click(Point)"/> method.
        /// </summary>
        [Fact]
        public void ClickCordsTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input tap 100 100"] = "Error: Injecting to another application requires INJECT_EVENTS permission\r\n";

            _ = Assert.Throws<ElementNotFoundException>(() => new DeviceClient(client, Device).Click(new Point(100, 100)));

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input tap 100 100", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.Swipe(int, int, int, int, long)"/> method.
        /// </summary>
        [Fact]
        public void SwipeTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input swipe 100 200 300 400 500"] = string.Empty;

            new DeviceClient(client, Device).Swipe(100, 200, 300, 400, 500);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input swipe 100 200 300 400 500", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.Swipe(Point, Point, long)"/> method.
        /// </summary>
        [Fact]
        public void SwipePointTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input swipe 100 200 300 400 500"] = string.Empty;

            new DeviceClient(client, Device).Swipe(new Point(100, 200), new Point(300, 400), 500);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input swipe 100 200 300 400 500", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.Swipe(Element, Element, long)"/> method.
        /// </summary>
        [Fact]
        public void SwipeElementTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input swipe 100 200 300 400 500"] = string.Empty;

            new DeviceClient(client, Device).Swipe(new Element(client, Device, new Rectangle(0, 0, 200, 400)), new Element(client, Device, new Rectangle(0, 0, 600, 800)), 500);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input swipe 100 200 300 400 500", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.IsAppRunning(string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("21216 27761\r\n", true)]
        [InlineData(" 21216 27761\r\n", true)]
        [InlineData("12836\r\n", true)]
        [InlineData(" \r\n", false)]
        [InlineData("\r\n", false)]
        [InlineData(" ", false)]
        [InlineData("", false)]
        public void IsAppRunningTest(string response, bool expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:pidof com.google.android.gms"] = response;

            bool result = new DeviceClient(client, Device).IsAppRunning("com.google.android.gms");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:pidof com.google.android.gms", client.ReceivedCommands[0]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.IsAppInForeground(string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", true)]
        [InlineData("com.android.settings", true)]
        [InlineData("com.google.android.gms", false)]
        public void IsAppInForegroundTest(string packageName, bool expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys activity activities | grep mResumedActivity"] =
                """
                    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
                    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}
                """;

            bool result = new DeviceClient(client, Device).IsAppInForeground(packageName);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:dumpsys activity activities | grep mResumedActivity", client.ReceivedCommands[0]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.GetAppStatus(string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("com.google.android.gms", "21216 27761\r\n", AppStatus.Background)]
        [InlineData("com.android.gallery3d", "\r\n", AppStatus.Stopped)]
        public void GetAppStatusTest(string packageName, string response, AppStatus expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys activity activities | grep mResumedActivity"] =
                """
                    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
                    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}
                """;
            client.Commands[$"shell:pidof {packageName}"] = response;

            AppStatus result = new DeviceClient(client, Device).GetAppStatus(packageName);

            Assert.Equal(2, client.ReceivedCommands.Count);
            Assert.Equal("shell:dumpsys activity activities | grep mResumedActivity", client.ReceivedCommands[0]);
            Assert.Equal($"shell:pidof {packageName}", client.ReceivedCommands[1]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.GetAppStatus(string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", AppStatus.Foreground)]
        [InlineData("com.android.settings", AppStatus.Foreground)]
        public void GetAppStatusForegroundTest(string packageName, AppStatus expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys activity activities | grep mResumedActivity"] =
                """
                    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
                    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}
                """;

            AppStatus result = new DeviceClient(client, Device).GetAppStatus(packageName);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:dumpsys activity activities | grep mResumedActivity", client.ReceivedCommands[0]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.FindElement(string, TimeSpan)"/> method.
        /// </summary>
        [Fact]
        public void FindElementTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.txt");

            Element element = new DeviceClient(client, Device).FindElement();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            Assert.Equal(144, element.GetChildCount());
            Element child = element[0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", child.Text);
            Assert.Equal("android.widget.TextView", child.Class);
            Assert.Equal("com.bilibili.app.in", child.Package);
            Assert.Equal("com.bilibili.app.in:id/header_info_name", child.ResourceID);
            Assert.Equal(Rectangle.FromLTRB(45, 889, 427, 973), child.Bounds);
            Assert.Equal(child, element.FindDescendantOrSelf(x => x.Text == "where-where"));
            Assert.Equal(2, element.FindDescendants().Where(x => x.Text == "where-where").Count());
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.FindElements(string, TimeSpan)"/> method.
        /// </summary>
        [Fact]
        public void FindElementsTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.txt");

            Element[] elements = new DeviceClient(client, Device).FindElements().ToArray();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            int childCount = elements.Length;
            Array.ForEach(elements, x => childCount += x.GetChildCount());
            Assert.Equal(145, childCount);
            Element element = elements[0][0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", element.Text);
            Assert.Equal(Rectangle.FromLTRB(45, 889, 427, 973), element.Bounds);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.SendKeyEvent(string)"/> method.
        /// </summary>
        [Fact]
        public void SendKeyEventTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input keyevent KEYCODE_MOVE_END"] = string.Empty;

            new DeviceClient(client, Device).SendKeyEvent("KEYCODE_MOVE_END");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input keyevent KEYCODE_MOVE_END", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.SendText(string)"/> method.
        /// </summary>
        [Fact]
        public void SendTextTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input text Hello, World"] = string.Empty;

            new DeviceClient(client, Device).SendText("Hello, World");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input text Hello, World", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.StartApp(string)"/> method.
        /// </summary>
        [Fact]
        public void StartAppTest()
        {
            DummyAdbClient client = new();

            new DeviceClient(client, Device).StartApp("com.android.settings");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:monkey -p com.android.settings 1", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.StopApp(string)"/> method.
        /// </summary>
        [Fact]
        public void StopAppTest()
        {
            DummyAdbClient client = new();

            new DeviceClient(client, Device).StopApp("com.android.settings");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:am force-stop com.android.settings", client.ReceivedCommands[0]);
        }
    }
}
