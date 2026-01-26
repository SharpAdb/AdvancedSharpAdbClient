#if !NET
// <copyright file="DateTimeExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="OperatingSystem"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class OperatingSystemExtensions
    {
        /// <summary>
        /// The extension for the <see cref="OperatingSystem"/> class.
        /// </summary>
        extension(OperatingSystem)
        {
            /// <summary>
            /// Indicates whether the current application is running on Windows.
            /// </summary>
            [MethodImpl((MethodImplOptions)0x100)]
            public static bool IsWindows() =>
#if NETCORE && !UAP10_0_15138_0
                true;
#elif !NETFRAMEWORK || NET48_OR_GREATER
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
                Environment.OSVersion.Platform
                    is PlatformID.Win32S
                    or PlatformID.Win32Windows
                    or PlatformID.Win32NT
                    or PlatformID.WinCE
                    or PlatformID.Xbox;
#endif

            /// <summary>
            /// Indicates whether the current application is running on Linux.
            /// </summary>
            [MethodImpl((MethodImplOptions)0x100)]
            public static bool IsLinux() =>
#if NETCORE && !UAP10_0_15138_0
                false;
#elif !NETFRAMEWORK || NET48_OR_GREATER
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
                Environment.OSVersion.Platform == PlatformID.Unix;
#endif

            /// <summary>
            /// Indicates whether the current application is running on macOS.
            /// </summary>
            [MethodImpl((MethodImplOptions)0x100)]
            public static bool IsMacOS() =>
#if NETCORE && !UAP10_0_15138_0
                false;
#elif !NETFRAMEWORK || NET48_OR_GREATER
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
                Environment.OSVersion.Platform == PlatformID.MacOSX;
#endif
            /// <summary>
            /// Indicates whether the current application is running on FreeBSD.
            /// </summary>
            [MethodImpl((MethodImplOptions)0x100)]
            public static bool IsFreeBSD() =>
#if NETCOREAPP3_0_OR_GREATER
                RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
#else
                false;
#endif
        }
    }
}
#endif