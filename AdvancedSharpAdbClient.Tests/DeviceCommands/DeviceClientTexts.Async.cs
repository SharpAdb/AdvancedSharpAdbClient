using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    public partial class DeviceExtensionsTests
    {
        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenStringAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task DumpScreenStringAsyncTest()
        {
            string dump = await File.ReadAllTextAsync(@"Assets/DumpScreen.txt");
            string cleanDump = await File.ReadAllTextAsync(@"Assets/DumpScreen.Clean.xml");

            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = dump;

            string xml = await new DeviceClient(client, Device).DumpScreenStringAsync();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            Assert.Equal(cleanDump, xml);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenStringAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task DumpScreenStringMIUIAsyncTest()
        {
            string miuidump = await File.ReadAllTextAsync(@"Assets/DumpScreen.MIUI.txt");
            string cleanMIUIDump = await File.ReadAllTextAsync(@"Assets/DumpScreen.Clean.xml");

            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = miuidump;

            string miuiXml = await new DeviceClient(client, Device).DumpScreenStringAsync();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            Assert.Equal(cleanMIUIDump, miuiXml);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenStringAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task DumpScreenStringEmptyAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = string.Empty;

            string emptyXml = await new DeviceClient(client, Device).DumpScreenStringAsync();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            Assert.True(string.IsNullOrEmpty(emptyXml));
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenStringAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task DumpScreenStringErrorAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = await File.ReadAllTextAsync(@"Assets/DumpScreen.Error.txt");

            _ = await Assert.ThrowsAsync<XmlException>(() => new DeviceClient(client, Device).DumpScreenStringAsync());

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task DumpScreenAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = await File.ReadAllTextAsync(@"Assets/DumpScreen.txt");

            XmlDocument xml = await new DeviceClient(client, Device).DumpScreenAsync();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            string cleanDump = await File.ReadAllTextAsync(@"Assets/DumpScreen.Clean.xml");
            XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc, xml);
        }

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Tests the <see cref="DeviceClient.DumpScreenWinRTAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task DumpScreenWinRTAsyncTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = await File.ReadAllTextAsync(@"Assets/DumpScreen.txt");

            Windows.Data.Xml.Dom.XmlDocument xml = await new DeviceClient(client, Device).DumpScreenWinRTAsync();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            string cleanDump = await File.ReadAllTextAsync(@"Assets/DumpScreen.Clean.xml");
            Windows.Data.Xml.Dom.XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc.InnerText, xml.InnerText);
        }

#endif

        /// <summary>
        /// Tests the <see cref="DeviceClient.ClickAsync(int, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ClickAsyncTest()
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

            JavaException exception = await Assert.ThrowsAsync<JavaException>(() => new DeviceClient(client, Device).ClickAsync(100, 100));

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
        /// Tests the <see cref="DeviceClient.ClickAsync(Point, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ClickCordsAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input tap 100 100"] = "Error: Injecting to another application requires INJECT_EVENTS permission\r\n";

            _ = await Assert.ThrowsAsync<ElementNotFoundException>(() => new DeviceClient(client, Device).ClickAsync(new Point(100, 100)));

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input tap 100 100", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.SwipeAsync(int, int, int, int, long, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SwipeAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input swipe 100 200 300 400 500"] = string.Empty;

            await new DeviceClient(client, Device).SwipeAsync(100, 200, 300, 400, 500);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input swipe 100 200 300 400 500", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.SwipeAsync(Point, Point, long, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SwipePointAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input swipe 100 200 300 400 500"] = string.Empty;

            await new DeviceClient(client, Device).SwipeAsync(new Point(100, 200), new Point(300, 400), 500);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input swipe 100 200 300 400 500", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.SwipeAsync(Element, Element, long, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SwipeElementAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input swipe 100 200 300 400 500"] = string.Empty;

            await new DeviceClient(client, Device).SwipeAsync(new Element(client, Device, new Rectangle(0, 0, 200, 400)), new Element(client, Device, new Rectangle(0, 0, 600, 800)), 500);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input swipe 100 200 300 400 500", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.IsAppRunningAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("21216 27761\r\n", true)]
        [InlineData(" 21216 27761\r\n", true)]
        [InlineData("12836\r\n", true)]
        [InlineData(" \r\n", false)]
        [InlineData("\r\n", false)]
        [InlineData(" ", false)]
        [InlineData("", false)]
        public async Task IsAppRunningAsyncTest(string response, bool expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:pidof com.google.android.gms"] = response;

            bool result = await new DeviceClient(client, Device).IsAppRunningAsync("com.google.android.gms");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:pidof com.google.android.gms", client.ReceivedCommands[0]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.IsAppInForegroundAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", true)]
        [InlineData("com.android.settings", true)]
        [InlineData("com.google.android.gms", false)]
        public async Task IsAppInForegroundAsyncTest(string packageName, bool expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys activity activities | grep mResumedActivity"] =
                """
                    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
                    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}
                """;

            bool result = await new DeviceClient(client, Device).IsAppInForegroundAsync(packageName);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:dumpsys activity activities | grep mResumedActivity", client.ReceivedCommands[0]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.GetAppStatusAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("com.google.android.gms", "21216 27761\r\n", AppStatus.Background)]
        [InlineData("com.android.gallery3d", "\r\n", AppStatus.Stopped)]
        public async Task GetAppStatusAsyncTest(string packageName, string response, AppStatus expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys activity activities | grep mResumedActivity"] =
                """
                    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
                    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}
                """;
            client.Commands[$"shell:pidof {packageName}"] = response;

            AppStatus result = await new DeviceClient(client, Device).GetAppStatusAsync(packageName);

            Assert.Equal(2, client.ReceivedCommands.Count);
            Assert.Equal("shell:dumpsys activity activities | grep mResumedActivity", client.ReceivedCommands[0]);
            Assert.Equal($"shell:pidof {packageName}", client.ReceivedCommands[1]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.GetAppStatusAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", AppStatus.Foreground)]
        [InlineData("com.android.settings", AppStatus.Foreground)]
        public async Task GetAppStatusForegroundAsyncTest(string packageName, AppStatus expected)
        {
            DummyAdbClient client = new();
            client.Commands["shell:dumpsys activity activities | grep mResumedActivity"] =
                """
                    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
                    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}
                """;

            AppStatus result = await new DeviceClient(client, Device).GetAppStatusAsync(packageName);

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:dumpsys activity activities | grep mResumedActivity", client.ReceivedCommands[0]);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.FindElementAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task FindElementAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.txt");

            Element element = await new DeviceClient(client, Device).FindElementAsync();

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
        /// Tests the <see cref="DeviceClient.FindElementsAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task FindElementsAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.txt");

            Element[] elements = (await new DeviceClient(client, Device).FindElementsAsync()).ToArray();

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
        /// Tests the <see cref="DeviceClient.FindAsyncElements(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task FindAsyncElementsTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:uiautomator dump /dev/tty"] = File.ReadAllText(@"Assets/DumpScreen.txt");

            List<Element> elements = await new DeviceClient(client, Device).FindAsyncElements().ToListAsync();

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:uiautomator dump /dev/tty", client.ReceivedCommands[0]);

            int childCount = elements.Count;
            elements.ForEach(x => childCount += x.GetChildCount());
            Assert.Equal(145, childCount);
            Element element = elements[0][0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", element.Text);
            Assert.Equal(Rectangle.FromLTRB(45, 889, 427, 973), element.Bounds);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.SendKeyEventAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SendKeyEventAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input keyevent KEYCODE_MOVE_END"] = string.Empty;

            await new DeviceClient(client, Device).SendKeyEventAsync("KEYCODE_MOVE_END");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input keyevent KEYCODE_MOVE_END", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.SendTextAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SendTextAsyncTest()
        {
            DummyAdbClient client = new();
            client.Commands["shell:input text Hello, World"] = string.Empty;

            await new DeviceClient(client, Device).SendTextAsync("Hello, World");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:input text Hello, World", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.StartAppAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StartAppAsyncTest()
        {
            DummyAdbClient client = new();

            await new DeviceClient(client, Device).StartAppAsync("com.android.settings");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:monkey -p com.android.settings 1", client.ReceivedCommands[0]);
        }

        /// <summary>
        /// Tests the <see cref="DeviceClient.StopAppAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StopAppAsyncTest()
        {
            DummyAdbClient client = new();

            await new DeviceClient(client, Device).StopAppAsync("com.android.settings");

            Assert.Single(client.ReceivedCommands);
            Assert.Equal("shell:am force-stop com.android.settings", client.ReceivedCommands[0]);
        }
    }
}
