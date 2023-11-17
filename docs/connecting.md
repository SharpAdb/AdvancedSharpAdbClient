# Connecting to device

## Running the adb server
AdvancedSharpAdbClient does not communicate directly with your Android devices, but uses the `adb.exe` server process as an intermediate. Before you can connect to your Android device, you must first start the `adb.exe` server.

To specify the path to adb and start the server use:
```csharp
StartServerResult startServerResult = await AdbServer.Instance.StartServerAsync("platform-tools\\adb.exe", false, CancellationToken.None);
```
If you started the server manually, no action is required

## Connecting to an emulator
If you are using an emulator, you need to know the ip and port to connect. Example for [Nox emulator](https://en.bignox.com/) it is `127.0.0.1:62001`
```csharp
var client = new AdbClient();
await client.ConnectAsync("127.0.0.1", 62001);
```

## Connecting to a real device via a USB cable
If you are using a real device, you need to enable `USB debugging` in the developer settings. Then you need to connect the device via cable and enable debugging on the phone. The device will automatically connect to ADB.
```csharp
var client = new AdbClient();
var devices = await client.GetDevicesAsync();
foreach (var device in devices)
{
    Console.WriteLine(device.Name);
}
```

## Connecting to a real device via wifi
Go to developer settings, select `Wifi debugging` and choose `Connect With Pairing Code`. Then you can pair your device like this
```csharp
var client = new AdbClient();
Console.Write("Enter pair host (like 127.0.0.1:1234): ");
var host = Console.ReadLine();
Console.Write("Enter pair code: ");
var pairCode = Console.ReadLine();
var result = await client.PairAsync(host, pairCode);
Console.WriteLine(result);
Console.Write("Enter connect host (like 127.0.0.1:4321): ");
var connectEndpoint = Console.ReadLine();
var connectHost = connectEndpoint.Split(":")[0];
var connectPort = int.Parse(connectEndpoint.Split(":")[1]);
await client.ConnectAsync(connectHost, connectPort);
```

> Please note that the connect IP and pair IP are usually the same, but the connect port and pair port are different.

## Events
You can also use the library to detect when the device is connecting, disconnecting, or changing.
```csharp
await using DeviceMonitor deviceMonitor = new(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
deviceMonitor.DeviceConnected += (sender, e) => Trace.WriteLine($"Device connected: {e.Device}");
deviceMonitor.DeviceDisconnected += (sender, e) => Trace.WriteLine($"Device disconnected: {e.Device}");
deviceMonitor.DeviceChanged += (sender, e) => Trace.WriteLine($"Device state changed: {e.Device} {e.OldState} -> {e.NewState}");
await deviceMonitor.StartAsync();
```

`DeviceMonitor` returns `DeviceData` that doesn't have a device model and name, and also can't be used for device automation. To obtain a `Device` that can be used further use the following code
```csharp
var client = new AdbClient();
await using DeviceMonitor deviceMonitor = new(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
deviceMonitor.DeviceConnected += async (s, deviceData) =>
{
    var devices = await client.GetDevicesAsync();
    var device = devices.First(d => d.Serial == deviceData.Device.Serial);
    Trace.WriteLine($"Device connected: {device.Model}");
};
await deviceMonitor.StartAsync();
```

Or you can directly monitor when device connected like this
```csharp
var client = new AdbClient();
var devices = await client.GetDevicesAsync();
while (!devices.Any(x => x.State == DeviceState.Online)) 
{
    Trace.WriteLine("Waiting for device connection..");
    devices = await client.GetDevicesAsync();
    await Task.Delay(1000);
}
var device = devices.FirstOrDefault();
Trace.WriteLine($"Connected {device.Name}");
```
