# Getting started

## Installation
### From nuget package manager
To install AdvancedSharpAdbClient install the [AdvancedSharpAdbClient NuGetPackage](https://www.nuget.org/packages/AdvancedSharpAdbClient). 
If you're using Visual Studio, you can run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console):
```powershell
PM> Install-Package AdvancedSharpAdbClient
```
### From main branch
You can install the actual version which is in the [main](https://github.com/SharpAdb/AdvancedSharpAdbClient) branch.
If you're using Visual Studio, you can run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console):
```powershell
PM> Install-Package AdvancedSharpAdbClient -Source https://nuget.pkg.github.com/SharpAdb/index.json
```
Then enter your github credentials and click OK.

!> This is a beta version and may contain bugs. Use it only to test new features and fixes before release.

## Connecting to device
### Running adb server
To specify the path to adb and start the server use:
```csharp
StartServerResult startServerResult = await AdbServer.Instance.StartServerAsync("platform-tools\\adb.exe", false, CancellationToken.None);
```
This will return one of these statuses:
```csharp
StartServerResult.Started
StartServerResult.AlreadyRunning
StartServerResult.RestartedOutdatedDaemon
```
If you started the server manually, no action is required