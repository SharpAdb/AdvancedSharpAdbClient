// <copyright file="DeviceState.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Defines the state of an Android device connected to the Android Debug Bridge.
    /// </summary>
    /// <remarks><para><seealso href="https://android.googlesource.com/platform/system/core/+/master/adb/adb.h"/></para>
    /// <para><seealso href="https://android.googlesource.com/platform/system/core/+/master/adb/transport.cpp"/></para></remarks>
    public enum DeviceState
    {
        /// <summary>
        /// The instance is not connected to adb or is not responding.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// The device is in bootloader mode
        /// </summary>
        BootLoader,

        /// <summary>
        /// The instance is now connected to the adb server. Note that this state does not imply that the Android system is
        /// fully booted and operational, since the instance connects to adb while the system is still booting.
        /// However, after boot-up, this is the normal operational state of an emulator/device instance.
        /// </summary>
        Online,

        /// <summary>
        /// The device is the adb host.
        /// </summary>
        Host,

        /// <summary>
        /// The device is in recovery mode.
        /// </summary>
        Recovery,

        /// <summary>
        /// Insufficient permissions to communicate with the device.
        /// </summary>
        NoPermissions,

        /// <summary>
        /// The device is in sideload mode.
        /// </summary>
        Sideload,

        /// <summary>
        /// The device is connected to adb, but adb is not authorized for remote debugging of this device.
        /// </summary>
        Unauthorized,

        /// <summary>
        /// The device is connected to adb, but adb authorizing for remote debugging of this device.
        /// </summary>
        Authorizing,

        /// <summary>
        /// The device state is unknown.
        /// </summary>
        Unknown
    }
}
