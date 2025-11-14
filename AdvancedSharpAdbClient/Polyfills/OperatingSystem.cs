#if !NETFRAMEWORK && !COMP_NETSTANDARD2_0
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Represents information about an operating system, such as the version and platform identifier. This class cannot be inherited.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class OperatingSystem;
}
#else
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.OperatingSystem))]
#endif