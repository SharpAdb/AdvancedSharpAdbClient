// <copyright file="PackageManagerReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace AdvancedSharpAdbClient.Receivers.DeviceCommands
{
    /// <summary>
    /// Parses the output of the various <c>pm</c> commands.
    /// </summary>
    /// <param name="packageManager">The parent package manager.</param>
    public class PackageManagerReceiver(PackageManager packageManager) : MultiLineReceiver
    {
        /// <summary>
        /// Gets the device.
        /// </summary>
        public DeviceData Device => PackageManager.Device;

        /// <summary>
        /// Gets the package manager.
        /// </summary>
        public PackageManager PackageManager { get; } = packageManager;

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
#if HAS_INDEXRANGE
                    string package = line[8..];
#else
                    string package = line.Substring(8);
#endif
                    //// If there's a '=' included, use the last instance,
                    //// to accommodate for values like
                    //// "package:/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk=com.google.android.apps.plus"
                    //string[] parts = line.Split(':', '=');

                    int separator = package.LastIndexOf('=');

                    if (separator == -1)
                    {
                        PackageManager.Packages[package] = string.Empty;
                    }
                    else
                    {
#if HAS_INDEXRANGE
                        string path = package[..separator];
                        string name = package[(separator + 1)..];
#else
                        string path = package.Substring(0, separator);
                        string name = package.Substring(separator + 1);
#endif
                        PackageManager.Packages[name] = path;
                    }
                }
            }
        }
    }
}
