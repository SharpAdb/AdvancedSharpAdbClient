| Issues | License | NuGet |
|--------|---------|-------|
[![Github Issues](https://img.shields.io/github/issues/SharpAdb/AdvancedSharpAdbClient)](https://github.com/SharpAdb/AdvancedSharpAdbClient/issues)|[![License](https://img.shields.io/github/license/SharpAdb/AdvancedSharpAdbClient)](https://github.com/SharpAdb/AdvancedSharpAdbClient/blob/main/LICENSE)|[![NuGet Status](https://img.shields.io/nuget/dt/AdvancedSharpAdbClient.svg?style=flat)](https://www.nuget.org/packages/AdvancedSharpAdbClient/)

# A .NET client for adb, the Android Debug Bridge (AdvancedSharpAdbClient)

AdvancedSharpAdbClient is a .NET library that allows .NET applications to communicate with Android devices. 
It provides a .NET implementation of the `adb` protocol, giving more flexibility to the developer than launching an 
`adb.exe` process and parsing the console output.

It's upgraded version of [SharpAdbClient](https://github.com/quamotion/madb).
Added important features.

## Support Platform
- .NET Framework 2.0 (Without Task)
- .NET Framework 3.5
- .NET Framework 4.0 (Need [Microsoft.Bcl.Async](https://www.nuget.org/packages/Microsoft.Bcl.Async))
- .NET Framework 4.5
- .NET Framework 4.6.1
- .NET Framework 4.8
- .NET Standard 1.3
- .NET Standard 2.0
- .NET Standard 2.1
- .NET Core 5.0 (Support UAP 10.0 and UAP 10.0.15138.0)
- .NET Core App 2.1
- .NET Core App 3.1
- .NET 6.0
- .NET 8.0

## Installation
To install AdvancedSharpAdbClient install the [AdvancedSharpAdbClient NuGetPackage](https://www.nuget.org/packages/AdvancedSharpAdbClient). If you're
using Visual Studio, you can run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console):

```ps
PM> Install-Package AdvancedSharpAdbClient
```

## Getting Started
AdvancedSharpAdbClient does not communicate directly with your Android devices, but uses the `adb.exe` server process as an intermediate. Before you can connect to your Android device, you must first start the `adb.exe` server.

You can do so by either running `adb.exe` yourself (it comes as a part of the ADK, the Android Development Kit), or you can use the `AdbServer.StartServer` method like this:

```cs
if (!AdbServer.Instance.GetStatus().IsRunning)
{
    AdbServer server = new AdbServer();
    StartServerResult result = server.StartServer(@"C:\adb\adb.exe", false);
    if (result != StartServerResult.Started)
    {
        Console.WriteLine("Can't start adb server");
    }
}
```

### Connecting to device
Before using all the methods, you must initialize the new AdbClient class and then connect to the device

If you want to automate 2 or more devices at the same time, you must remember: 1 device - 1 AdbClient class

You can look at the examples to understand more

```cs
static AdbClient adbClient;
static DeviceData deviceData;

static void Main(string[] args)
{
    adbClient = new AdbClient();
    adbClient.Connect("127.0.0.1:62001");
    device = adbClient.GetDevices().FirstOrDefault(); // Get first connected device
    ...
}
```

## Device automation

### Finding element
You can find the element on the screen by xpath

```cs
static DeviceClient deviceClient;

static void Main(string[] args)
{
    ...
    deviceClient = new DeviceClient(adbClient, deviceData);
    Element element = deviceClient.FindElement("//node[@text='Login']");
    ...
}
```

You can also specify the waiting time for the element

```cs
Element element = deviceClient.FindElement("//node[@text='Login']", TimeSpan.FromSeconds(5));
```

And you can find several elements

```cs
Element[] element = deviceClient.FindElements("//node[@resource-id='Login']", TimeSpan.FromSeconds(5));
```

### Getting element attributes
You can get all element attributes

```cs
static void Main(string[] args)
{
    ...
    Element element = deviceClient.FindElement("//node[@resource-id='Login']", TimeSpan.FromSeconds(3));
    string eltext = element.Attributes["text"];
    string bounds = element.Attributes["bounds"];
    ...
}
```


### Clicking on an element
You can click on the x and y coordinates

```cs
static void Main(string[] args)
{
    ...
    deviceClient.Click(600, 600); // Click on the coordinates (600;600)
    ...
}
```

Or on the element(need xpath)

```cs
static void Main(string[] args)
{
    ...
    Element element = deviceClient.FindElement("//node[@text='Login']", TimeSpan.FromSeconds(3));
    element.Click(); // Click on element by xpath //node[@text='Login']
    ...
}
```

The Click() method throw ElementNotFoundException if the element is not found

```cs
try
{
    element.Click();
}
catch (Exception ex)
{
    Console.WriteLine($"Can't click on the element:{ex.Message}");
}
```


### Swipe
You can swipe from one element to another

```cs
static void Main(string[] args)
{
    ...
    Element first = deviceClient.FindElement("//node[@text='Login']");
    Element second = deviceClient.FindElement("//node[@text='Password']");
    deviceClient.Swipe(first, second, 100); // Swipe 100 ms
    ...
}
```

Or swipe by coordinates

```cs
static void Main(string[] args)
{
    ...
    deviceClient.Swipe(600, 1000, 600, 500, 100); // Swipe from (600;1000) to (600;500) on 100 ms
    ...
}
```

The Swipe() method throw ElementNotFoundException if the element is not found

```cs
try
{
    deviceClient.Swipe(0x2232323, 0x954, 0x9128, 0x11111, 200);
    ...
    deviceClient.Swipe(first, second, 200);
}
catch (Exception ex)
{
    Console.WriteLine($"Can't swipe:{ex.Message}");
}
```


### Send text
You can send any text except Cyrillic (Russian isn't supported by adb)

The text field should be in focus

```cs
static void Main(string[] args)
{
    ...
    deviceClient.SendText("text"); // Send text to device
    ...
}
```

You can also send text to the element (clicks on the element and sends the text)

```cs
static void Main(string[] args)
{
    ...
    deviceClient.FindElement("//node[@resource-id='Login']").SendText("text"); // Send text to the element by xpath //node[@resource-id='Login']
    ...
}
```

The SendText() method throw InvalidTextException if text is incorrect

```cs
try
{
    deviceClient.SendText(null);
}
catch (Exception ex)
{
    Console.WriteLine($"Can't send text:{ex.Message}");
}
```


### Clearing the input text

You can clear text input

The text field should be in focus

**Recommended**

```cs
static void Main(string[] args)
{
    ...
    deviceClient.ClearInput(25); // The argument is to specify the maximum number of characters to be erased
    ...
}
```

**It may work unstable**

```cs
static void Main(string[] args)
{
    ...
    deviceClient.FindElement("//node[@resource-id='Login']").ClearInput(); // Get element text attribute and remove text length symbols
    ...
}
```

### Sending key events

You can see key events here https://developer.android.com/reference/android/view/KeyEvent#constants

```cs
static void Main(string[] args)
{
    ...
    deviceClient.SendKeyEvent("KEYCODE_TAB");
    ...
}
```

The SendKeyEvent method throw InvalidKeyEventException if key event is incorrect

```cs
try
{
    deviceClient.SendKeyEvent(null);
}
catch (Exception ex)
{
    Console.WriteLine($"Can't send keyevent:{ex.Message}");
}
```

### BACK and HOME buttons

```cs
static void Main(string[] args)
{
    ...
    deviceClient.ClickBackButton(); // Click Back button
    ...
    deviceClient.ClickHomeButton(); // Click Home button
    ...
}
```

## Device commands
**Some commands require Root**
### Install and Uninstall applications

```cs
static void Main(string[] args)
{
    ...
    PackageManager manager = new PackageManager(adbClient, deviceData);
    manager.InstallPackage(@"C:\Users\me\Documents\mypackage.apk");
    manager.UninstallPackage("com.android.app");
    ...
}
```

Or you can use AdbClient.Install

```cs
static void Main(string[] args)
{
    ...
    using (FileStream stream = File.OpenRead("Application.apk"))
    {
        adbClient.Install(device, stream);
        adbClient.Uninstall(device, "com.android.app");
    }
    ...
}
```
### Install multiple applications

```cs
static void Main(string[] args)
{
    ...
    PackageManager manager = new PackageManager(adbClient, deviceData);
    manager.InstallMultiplePackage(@"C:\Users\me\Documents\base.apk", new[] { @"C:\Users\me\Documents\split_1.apk", @"C:\Users\me\Documents\split_2.apk" }); // Install split app whith base app
    manager.InstallMultiplePackage(new[] { @"C:\Users\me\Documents\split_3.apk", @"C:\Users\me\Documents\split_4.apk" }, "com.android.app"); // Add split app to base app which packagename is 'com.android.app'
    ...
}
```

Or you can use AdbClient.InstallMultiple

```cs
static void Main(string[] args)
{
    ...
    adbClient.InstallMultiple(device, File.OpenRead("base.apk"), new[] { File.OpenRead("split_1.apk"), File.OpenRead("split_2.apk") }); // Install split app whith base app
    adbClient.InstallMultiple(device, new[] { File.OpenRead("split_3.apk"), File.OpenRead("split_4.apk") }, "com.android.app"); // Add split app to base app which packagename is 'com.android.app'
    ...
}
```

### Start and stop applications

```cs
static void Main(string[] args)
{
    ...
    deviceClient.StartApp("com.android.app");
    deviceClient.StopApp("com.android.app"); // force-stop
    ...
}
```

### Getting a screenshot

```cs
static async void Main(string[] args)
{
    ...
    Image img = adbClient.GetFrameBuffer(deviceData, CancellationToken.None); // synchronously
    ...
    Image img = await adbClient.GetFrameBufferAsync(deviceData, CancellationToken.None); // asynchronously
    ...
}
```

### Getting screen xml hierarchy

```cs
static void Main(string[] args)
{
    ...
    XmlDocument screen = deviceClient.DumpScreen();
    ...
}
```

### Send or receive files

```cs
void DownloadFile()
{
    using (SyncService service = new SyncService(deviceData))
    {
        using (FileStream stream = File.OpenWrite(@"C:\MyFile.txt"))
        {
            service.Pull("/data/local/tmp/MyFile.txt", stream, null);
        }
    }
}

void UploadFile()
{
    using (SyncService service = new SyncService(deviceData))
    {
        using (FileStream stream = File.OpenRead(@"C:\MyFile.txt"))
        {
            service.Push(stream, "/data/local/tmp/MyFile.txt", UnixFileStatus.DefaultFileMode, DateTimeOffset.Now, null);
        }
    }
}
```

### Run shell commands

```cs
static async void Main(string[] args)
{
    ...
    ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
    adbClient.ExecuteRemoteCommand("echo Hello, World", device, receiver); // synchronously
    ...
    await adbClient.ExecuteRemoteCommandAsync("echo Hello, World", device, receiver, default); // asynchronously
    ...
}
```

### Encoding
Default encoding is UTF8, if you want to change it, use

```cs
AdbClient.SetEncoding(Encoding.ASCII);
```

## Contributors
[![Contributors](https://contrib.rocks/image?repo=SharpAdb/AdvancedSharpAdbClient)](https://github.com/SharpAdb/AdvancedSharpAdbClient/graphs/contributors)

## Consulting and Support
Please open an [**issue**](https://github.com/SharpAdb/AdvancedSharpAdbClient/issues) on if you have suggestions or problems.

## History
AdvancedSharpAdbClient is a fork of [SharpAdbClient](https://github.com/quamotion/madb) and [madb](https://github.com/camalot/madb) which in itself is a .NET port of the [ddmlib Java Library](https://android.googlesource.com/platform/tools/base/+/master/ddmlib/).

Credits:
https://github.com/camalot, https://github.com/quamotion