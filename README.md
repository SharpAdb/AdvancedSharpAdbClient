# A .NET client for adb, the Android Debug Bridge (AdvancedSharpAdbClient)

AdvancedSharpAdbClient is a .NET library that allows .NET applications to communicate with Android devices. 
It provides a .NET implementation of the `adb` protocol, giving more flexibility to the developer than launching an 
`adb.exe` process and parsing the console output.

It's upgraded verion of [SharpAdbClient](https://github.com/quamotion/madb).
Added important features.

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

## Using AdvancedAdbClient library

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
static void Main(string[] args)
{
	...
	Element el = client.FindElement(device, "//node[@resource-id='Login']", TimeSpan.FromSeconds(3));
	string eltext = el.attributes["text"];
	string bounds = el.attributes["bounds"];
	...
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
static void Main(string[] args)
{
    ...
    Element first = client.FindElement(device, "//node[@text='Login']");
    Element second = client.FindElement(device, "//node[@text='Password']");
    client.Swipe(device, first, second, 100); // Swipe 100 ms
	...
}
```

Or swipe by coordinates

```c#
static void Main(string[] args)
{
	...
    device = client.GetDevices().FirstOrDefault();
    client.Swipe(device, 600, 1000, 600, 500, 100); // Swipe from (600;1000) to (600;500) on 100 ms
	...
}
```

The Swipe() method returns true if the click was successful and false if it failed

```c#
if (!client.Swipe(device, 600, 1000, 600, 500, 100))
{
	Console.WriteLine("Can't swipe");
}
```

### Send text
You can send any text except Cyrillic (Russian isn't supported by adb)

The text field should be in focus

```c#
static void Main(string[] args)
{
    ...
    client.SendText(device, "text"); // Send text to device
	...
}
```

You can also send text to the element (clicks on the element and sends the text)

```c#
static void Main(string[] args)
{
    ...
    client.FindElement(device, "//node[@resource-id='Login']").SendText("text"); // Send text to the element by xpath //node[@resource-id='Login']
	...
}
```

The SendText() method returns true if the click was successful and false if it failed

```c#
if (!client.SendText(device, "text"))
{
	Console.WriteLine("Can't send text");
}
```

### Clearing the input text

You can clear text input

The text field should be in focus

Recommended
```c#
static void Main(string[] args)
{
    ...
    client.ClearInput(device, 25); // The second argument is to specify the maximum number of characters to be erased
	...
}
```

It may work unstable
```c#
static void Main(string[] args)
{
    ...
    client.FindElement(device, "//node[@resource-id='Login']").ClearInput(); // Get element text attribute and remove text length symbols
	...
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

