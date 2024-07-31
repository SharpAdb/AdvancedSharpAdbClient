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

## Quick start
AdvancedSharpAdbClient does not communicate directly with your Android devices, but uses the `adb.exe` server process as an intermediate.

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

Before using all the methods, you must initialize the new AdbClient class and then connect to the device.

You can look at the example to understand more

```cs
static AdbClient adbClient;
static DeviceData deviceData;

static void Main(string[] args)
{
    adbClient = new AdbClient();
    adbClient.ConnectAsync("127.0.0.1", 62001);
    device = adbClient.GetDevices().FirstOrDefault();
    Element element = await adbClient.FindElementAsync(device, "//node[@text='Login']");
    await element.ClickAsync();
    ...
}
```

### Documentation
You can find documentation at https://sharpadb.github.io.

You can also view the [SampleApp](https://github.com/SharpAdb/AdvancedSharpAdbClient.SampleApp) for more examples.


## Contributors
[![Contributors](https://contrib.rocks/image?repo=SharpAdb/AdvancedSharpAdbClient)](https://github.com/SharpAdb/AdvancedSharpAdbClient/graphs/contributors)

## Consulting and Support
Please open an [**issue**](https://github.com/SharpAdb/AdvancedSharpAdbClient/issues) on if you have suggestions or problems.

## History
AdvancedSharpAdbClient is a fork of [SharpAdbClient](https://github.com/quamotion/madb) and [madb](https://github.com/camalot/madb) which in itself is a .NET port of the [ddmlib Java Library](https://android.googlesource.com/platform/tools/base/+/master/ddmlib/).

Credits:
https://github.com/camalot, https://github.com/quamotion
