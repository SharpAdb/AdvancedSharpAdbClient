﻿// <copyright file="ForwardProtocol.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Represents a protocol which is being forwarded over adb.
    /// </summary>
    public enum ForwardProtocol : byte
    {
        /// <summary>
        /// Enables the forwarding of a TCP port.
        /// </summary>
        Tcp,

        /// <summary>
        /// Enables the forwarding of a Unix domain socket.
        /// </summary>
        LocalAbstract,

        /// <summary>
        /// Enables the forwarding of a Unix domain socket.
        /// </summary>
        LocalReserved,

        /// <summary>
        /// Enables the forwarding of a Unix domain socket.
        /// </summary>
        LocalFilesystem,

        /// <summary>
        /// Enables the forwarding of a character device.
        /// </summary>
        Device,

        /// <summary>
        /// Enables port forwarding of the java debugger for a specific process.
        /// </summary>
        JavaDebugWireProtocol
    }
}
