// <copyright file="PackageManagerReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers
{
    /// <summary>
    /// Parses the output of the various <c>pm</c> commands.
    /// </summary>
    /// <param name="packageManager">The parent package manager.</param>
    [DebuggerDisplay($"{nameof(PackageManagerReceiver)} \\{{ {nameof(PackageManager)} = {{{nameof(PackageManager)}}} }}")]
    public class PackageManagerReceiver(PackageManager packageManager) : MultiLineReceiver
    {
        /// <summary>
        /// Gets the device.
        /// </summary>
        public DeviceData Device => PackageManager.Device;

        /// <summary>
        /// Gets the package manager.
        /// </summary>
        public PackageManager PackageManager => packageManager;

        /// <inheritdoc/>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            PackageManager.Packages.Clear();

            foreach (string line in lines)
            {
                if (line != null && line.StartsWith("package:"))
                {
                    // Samples include:
                    // package:/system/app/LegacyCamera.apk=com.android.camera
                    // package:mwc2015.be

                    // Remove the "package:" prefix
                    string package = line[8..];

                    // If there's a '=' included, use the last instance,
                    // to accommodate for values like
                    // "package:/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk=com.google.android.apps.plus"
                    int separator = package.LastIndexOf('=');

                    if (separator == -1)
                    {
                        PackageManager.Packages[package] = string.Empty;
                    }
                    else
                    {
                        string path = package[..separator++];
                        string name = package[separator..];
                        PackageManager.Packages[name] = path;
                    }
                }
            }
        }
    }
}
