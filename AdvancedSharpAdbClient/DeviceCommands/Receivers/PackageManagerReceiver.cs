// <copyright file="PackageManagerReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Parses the output of the various <c>pm</c> commands.
    /// </summary>
    public class PackageManagerReceiver : MultiLineReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManagerReceiver"/> class.
        /// </summary>
        /// <param name="device">The device for which the package information is being received.</param>
        /// <param name="packageManager">The parent package manager.</param>
        public PackageManagerReceiver(DeviceData device, PackageManager packageManager)
        {
            Device = device;
            PackageManager = packageManager;
        }

        /// <summary>
        /// Gets the device.
        /// </summary>
        public DeviceData Device { get; private set; }

        /// <summary>
        /// Gets the package manager.
        /// </summary>
        public PackageManager PackageManager { get; private set; }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
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
#if NETCOREAPP
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
                        PackageManager.Packages.Add(package, null);
                    }
                    else
                    {
#if NETCOREAPP
                        string path = package[..separator];
                        string name = package[(separator + 1)..];
#else
                        string path = package.Substring(0, separator);
                        string name = package.Substring(separator + 1);
#endif
                        PackageManager.Packages.Add(name, path);
                    }
                }
            }
        }
    }
}
