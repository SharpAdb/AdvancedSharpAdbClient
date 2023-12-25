#region AdvancedSharpAdbClient
global using AdvancedSharpAdbClient.DeviceCommands.Models;
global using AdvancedSharpAdbClient.DeviceCommands.Receivers;
global using AdvancedSharpAdbClient.Exceptions;
global using AdvancedSharpAdbClient.Logs;
global using AdvancedSharpAdbClient.Models;
global using AdvancedSharpAdbClient.Polyfills;
global using AdvancedSharpAdbClient.Receivers;
#endregion

#region AdvancedSharpAdbClient.Tests
global using AdvancedSharpAdbClient.Tests;
global using AdvancedSharpAdbClient.Logs.Tests;
#endregion

#if WINDOWS10_0_17763_0_OR_GREATER
global using Windows.ApplicationModel;
global using System.Runtime.InteropServices.WindowsRuntime;
global using Windows.Storage;
global using Windows.Storage.Streams;
#endif