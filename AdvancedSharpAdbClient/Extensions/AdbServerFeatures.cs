// <copyright file="AdbServerFeatures.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Lists features which an Android Debug Bridge can support.
    /// </summary>
    public static class AdbServerFeatures
    {
        /// <summary>
        /// The server supports the shell protocol.
        /// </summary>
        public const string Shell2 = "shell_v2";

        /// <summary>
        /// The server supports the <c>cmd</c> command.
        /// </summary>
        public const string Cmd = "cmd";

        /// <summary>
        /// The server supports the stat2 protocol.
        /// </summary>
        public const string Stat2 = "stat_v2";

        /// <summary>
        /// The server supports libusb.
        /// </summary>
        public const string LibUsb = "libusb";

        /// <summary>
        /// The server supports <c>push --sync</c>.
        /// </summary>
        public const string PushSync = "push_sync";
    }
}
