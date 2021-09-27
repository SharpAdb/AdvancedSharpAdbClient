# A .NET client for adb, the Android Debug Bridge (AdvancedSharpAdbClient)

AdvancedSharpAdbClient is a .NET library that allows .NET applications to communicate with Android devices. 
It provides a .NET implementation of the `adb` protocol, giving more flexibility to the developer than launching an 
`adb.exe` process and parsing the console output.

It's upgraded verion of 

## Installation
To install AdvancedSharpAdbClient install the [AdvancedSharpAdbClient NuGetPackage](https://www.nuget.org/packages/AdvancedSharpAdbClient). If you're
using Visual Studio, you can run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console):

```
PM> Install-Package AdvancedSharpAdbClient
```

## Getting Started
AdvancedSharpAdbClient does not communicate directly with your Android devices, but uses the `adb.exe` server process as an intermediate. Before you can connect to your Android device, you must first start the `adb.exe` server.

You can do so by either running `adb.exe` yourself (it comes as a part of the ADK, the Android Development Kit), or you can use the `AdbServer.StartServer` method like this:

```c#
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
Before using all the methods, you must initialize the new AdvancedAdbClient class and then connect to the device

If you want to automate 2 or more devices at the same time, you must remember: 1 device - 1 AdvancedAdbClient class

You can look at the examples to understand more

```c#
static AdvancedAdbClient client;

static DeviceData device;

static void Main(string[] args)
{
	client = new AdvancedAdbClient();
	client.Connect("127.0.0.1:62001");
	device = client.GetDevices().FirstOrDefault();
}
```

### Finding element
You can find the element on the screen by xpath

```c#
static AdvancedAdbClient client;

static DeviceData device;

static void Main(string[] args)
{
	client = new AdvancedAdbClient();
	client.Connect("127.0.0.1:62001");
	device = client.GetDevices().FirstOrDefault();
	Element el = client.FindElement(device, "//node[@text='Login']");
}
```

You can also specify the waiting time for the element

```c#
Element el = client.FindElement(device, "//node[@text='Login']", TimeSpan.FromSeconds(5));
```

You can also find several elements

```
Element[] els = client.FindElements(device, "//node[@resource-id='Login']", TimeSpan.FromSeconds(5));
```

### Getting element attributes
You can get all element attributes


```c#
static AdvancedAdbClient client;

static DeviceData device;

static void Main(string[] args)
{
	client = new AdvancedAdbClient();
	client.Connect("127.0.0.1:62001");
	device = client.GetDevices().FirstOrDefault();
	Element el = client.FindElement(device, "//node[@resource-id='Login']", TimeSpan.FromSeconds(3));
	
	string eltext = el.attributes["text"];
	string bounds = el.attributes["bounds"];
}
```

### Clicking on an element
To click on an element you need AdvancedAdbClient and DeviceData saved in the previous example

You can click on the x and y coordinates, or on the element(need xpath)

```c#
static AdvancedAdbClient client;

static DeviceData device;

static void ClickOnCoordinates()
{
    client.Click(device, 600, 600); // Click on the coordinates (600;600)
}

static void ClickOnElement()
{
    Element el = client.FindElement(device, "//node[@text='Login']", TimeSpan.FromSeconds(3)); // Click on element by xpath //node[@text='Login']
    el.Click();
}

static void Main(string[] args)
{
    client = new AdvancedAdbClient();
    client.Connect("127.0.0.1:62001");
    device = client.GetDevices().FirstOrDefault();
    ClickOnCoordinates();
    ClickOnElement();
}
```

The Click() method returns true if the click was successful and false if it failed

```c#
if (!el.Click())
{
	Console.WriteLine("Can't click on an element");
}
```

### Swipe
You can swipe from one element to another

```c#
static AdvancedAdbClient client;

static DeviceData device;

static void Main(string[] args)
{
    client = new AdvancedAdbClient();
    client.Connect("127.0.0.1:62001");
    device = client.GetDevices().FirstOrDefault();
    Element first = client.FindElement(device, "//node[@text='Login']");
    Element second = client.FindElement(device, "//node[@text='Password']");
    client.Swipe(device, first, second, 100); // Swipe 100 ms
}
```

Or swipe by coordinates

```c#
static AdvancedAdbClient client;

static DeviceData device;

static void Main(string[] args)
{
    client = new AdvancedAdbClient();
    client.Connect("127.0.0.1:62001");
    device = client.GetDevices().FirstOrDefault();
    client.Swipe(device, 600, 1000, 600, 500, 100); // Swipe from (600;1000) to (600;500) on 100 ms
}
```

### Send text
You can send any text except Cyrillic (Russian isn't supported by adb)

```c#
void DownloadFile()
{
    var device = AdbClient.Instance.GetDevices().First();
    
    using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
    using (Stream stream = File.OpenWrite(@"C:\MyFile.txt"))
    {
        service.Pull("/data/local/tmp/MyFile.txt", stream, null, CancellationToken.None);
    }
}

void UploadFile()
{
    var device = AdbClient.Instance.GetDevices().First();
    
    using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
    using (Stream stream = File.OpenRead(@"C:\MyFile.txt"))
    {
        service.Push(stream, "/data/local/tmp/MyFile.txt", 444, DateTime.Now, null, CancellationToken.None);
    }
}
```

### Run shell commands
To run shell commands on an Android device, you can use the `AdbClient.Instance.ExecuteRemoteCommand` method.

You need to pass a `DeviceData` object which specifies the device on which you want to run your command. You
can get a `DeviceData` object by calling `AdbClient.Instance.GetDevices()`, which will run one `DeviceData`
object for each device Android connected to your PC.

You'll also need to pass an `IOutputReceiver` object. Output receivers are classes that receive and parse the data
the device sends back. In this example, we'll use the standard `ConsoleOutputReceiver`, which reads all console
output and allows you to retrieve it as a single string. You can also use other output receivers or create your own.

```
void EchoTest()
{
    var device = AdbClient.Instance.GetDevices().First();
    var receiver = new ConsoleOutputReceiver();

    AdbClient.Instance.ExecuteRemoteCommand("echo Hello, World", device, receiver);

    Console.WriteLine("The device responded:");
    Console.WriteLine(receiver.ToString());
}
```

## Consulting, Training and Support
This repository is maintained by [Quamotion](http://quamotion.mobi). Quamotion develops test software for iOS and 
Android applications, based on the WebDriver protocol.

In certain cases, Quamotion also offers professional services - such as consulting, training and support - related
to AdvancedSharpAdbClient. Contact us at [info@quamotion.mobi](mailto:info@quamotion.mobi) for more information.

## History
AdvancedSharpAdbClient is a fork of [madb](https://github.com/camalot/madb); which in itself is a .NET port of the 
[ddmlib Java Library](https://android.googlesource.com/platform/tools/base/+/master/ddmlib/). Credits for porting 
this library go to [Ryan Conrad](https://github.com/camalot).

