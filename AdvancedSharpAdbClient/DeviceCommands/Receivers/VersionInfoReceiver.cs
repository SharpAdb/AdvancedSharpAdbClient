// <copyright file="VersionInfoReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Processes command line output of the <c>dumpsys package</c> command.
    /// </summary>
    internal class VersionInfoReceiver : InfoReceiver
    {
        /// <summary>
        /// The name of the version code property.
        /// </summary>
        private static readonly string versionCode = "VersionCode";

        /// <summary>
        /// The name of the version name property.
        /// </summary>
        private static readonly string versionName = "VersionName";

        /// <summary>
        /// Tracks whether we're currently in the packages section or not.
        /// </summary>
        private bool inPackagesSection = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfoReceiver"/> class.
        /// </summary>
        public VersionInfoReceiver()
        {
            AddPropertyParser(versionCode, GetVersionCode);
            AddPropertyParser(versionName, GetVersionName);
        }

        /// <summary>
        /// Gets the version code of the specified package.
        /// </summary>
        public VersionInfo VersionInfo =>
            GetPropertyValue(versionCode) != null && GetPropertyValue(versionName) != null
                ? new VersionInfo((int)GetPropertyValue(versionCode), (string)GetPropertyValue(versionName))
                : null;

        private void CheckPackagesSection(string line)
        {
            // This method check whether we're in the packages section of the dumpsys package output.
            // See gapps.txt for what the output looks for. Each section starts with a header
            // which looks like:
            //
            // HeaderName:
            //
            // and then there's indented data.

            // We check whether the line is indented. If it's not, and it's not an empty line, we take it is
            // a section header line and update the data accordingly.
            if (line.IsNullOrWhiteSpace())
            {
                return;
            }

            if (char.IsWhiteSpace(line[0]))
            {
                return;
            }

            inPackagesSection = string.Equals("Packages:", line, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses the given line and extracts the version name if possible.
        /// </summary>
        /// <param name="line">The line to be parsed.</param>
        /// <returns>The extracted version name.</returns>
        internal object GetVersionName(string line)
        {
            CheckPackagesSection(line);

            return !inPackagesSection ? null
                : line != null && line.Trim().StartsWith("versionName=")
#if HAS_INDEXRANGE
                ? line.Trim()[12..].Trim() : null;
#else
                ? line.Trim().Substring(12).Trim() : null;
#endif
        }

        /// <summary>
        /// Parses the given line and extracts the version code if possible.
        /// </summary>
        /// <param name="line">The line to be parsed.</param>
        /// <returns>The extracted version code.</returns>
        internal object GetVersionCode(string line)
        {
            CheckPackagesSection(line);

            if (!inPackagesSection)
            {
                return null;
            }

            if (line == null)
            {
                return null;
            }

            // versionCode=4 minSdk=9 targetSdk=22
            string versionCodeRegex = @"versionCode=(\d*)( minSdk=(\d*))?( targetSdk=(\d*))?$";
            Match match = Regex.Match(line, versionCodeRegex);
            return match.Success ? int.Parse(match.Groups[1].Value.Trim()) : null;
        }
    }
}
