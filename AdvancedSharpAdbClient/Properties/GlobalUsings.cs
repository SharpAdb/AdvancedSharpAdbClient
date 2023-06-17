#if NET
global using System.Runtime.Versioning;
#endif

#if NET8_0_OR_GREATER
global using System.Numerics;
#endif

#if WINDOWS_UWP
global using Windows.Foundation;
global using Windows.Foundation.Metadata;
global using Windows.Graphics.Imaging;
global using Windows.Storage.Streams;
global using Windows.System;
global using Windows.UI.Core;
global using Windows.UI.Xaml.Media.Imaging;
global using Buffer = System.Buffer;
global using DateTime = System.DateTime;
global using TimeSpan = System.TimeSpan;
#endif

#if WINDOWS10_0_17763_0_OR_GREATER
global using Windows.Foundation;
global using Buffer = System.Buffer;
global using DateTime = System.DateTime;
global using TimeSpan = System.TimeSpan;
#endif

#if HAS_TASK
global using System.Threading.Tasks;
#endif

#if HAS_LOGGER
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
#endif

#if HAS_BUFFERS
global using System.Buffers;
#endif

#if HAS_SERIALIZATION
global using System.Runtime.Serialization;
#endif

#if HAS_DRAWING
global using System.Drawing;
global using System.Drawing.Imaging;
global using System.Runtime.InteropServices;
#endif
