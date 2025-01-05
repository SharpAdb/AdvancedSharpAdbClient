#region AdvancedSharpAdbClient
global using AdvancedSharpAdbClient.DeviceCommands;
global using AdvancedSharpAdbClient.DeviceCommands.Models;
global using AdvancedSharpAdbClient.DeviceCommands.Receivers;
global using AdvancedSharpAdbClient.Exceptions;
global using AdvancedSharpAdbClient.Logs;
global using AdvancedSharpAdbClient.Models;
global using AdvancedSharpAdbClient.Polyfills;
global using AdvancedSharpAdbClient.Receivers;
#endregion

#if NET
global using System.Runtime.Versioning;
#endif

#if NET8_0_OR_GREATER
global using System.Numerics;
#endif

#if HAS_WINRT
global using System.Runtime.InteropServices.WindowsRuntime;
global using Windows.ApplicationModel;
global using Windows.Foundation;
global using Windows.Foundation.Metadata;
global using Windows.Storage;
global using Windows.Storage.Streams;
global using Buffer = System.Buffer;
global using DateTime = System.DateTime;
global using Point = System.Drawing.Point;
global using TimeSpan = System.TimeSpan;
#endif

#if HAS_WUXC
global using Windows.Graphics.Imaging;
global using Windows.System;
global using Windows.UI.Core;
global using Windows.UI.Xaml.Media.Imaging;
#endif

#if HAS_TASK
global using System.Threading.Tasks;
#endif

#if HAS_BUFFERS
global using System.Buffers;
#endif

#if HAS_IMAGING
global using System.Drawing.Imaging;
#endif

#if HAS_SERIALIZATION
global using System.Runtime.Serialization;
#endif
