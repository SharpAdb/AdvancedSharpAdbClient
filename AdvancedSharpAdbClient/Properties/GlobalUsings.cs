#if NET
global using System.Runtime.Versioning;
#endif

#if NET8_0_OR_GREATER
global using System.Numerics;
#endif

#if WINDOWS_UWP
global using Windows.System;
global using Windows.UI.Core;
global using Windows.UI.Xaml.Media.Imaging;
#endif

#if HAS_LOGGER
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
#endif

#if HAS_SERIALIZATION
global using System.Runtime.Serialization;
#endif

#if HAS_DRAWING
global using System.Drawing;
#endif
