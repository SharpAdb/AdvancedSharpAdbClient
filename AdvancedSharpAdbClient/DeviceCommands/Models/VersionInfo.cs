// <copyright file="VersionInfo.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AdvancedSharpAdbClient.DeviceCommands.Models
{
    /// <summary>
    /// Represents a version of an Android application.
    /// </summary>
    /// <param name="VersionCode">The version code of the application.</param>
    /// <param name="VersionName">The version name of the application.</param>
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public readonly record struct VersionInfo(int VersionCode, string VersionName) : IComparer, IComparer<VersionInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> struct.
        /// </summary>
        public VersionInfo() : this(0, "Unknown") { }

        /// <summary>
        /// Gets or sets the version code of an Android application.
        /// </summary>
        public int VersionCode { get; init; } = VersionCode;

        /// <summary>
        /// Gets or sets the version name of an Android application.
        /// </summary>
        public string VersionName { get; init; } = VersionName;

        /// <summary>
        /// Try to parse the <see cref="VersionName"/> into a <see cref="Version"/> object.
        /// </summary>
        /// <param name="version">The <see cref="Version"/> object.</param>
        /// <returns><see langword="true"/> if the <see cref="VersionName"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
        public readonly bool TryAsVersion(out Version? version)
        {
            int[] numbs = GetVersionNumbers(VersionName).Split('.', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Take(4).ToArray();
            switch (numbs.Length)
            {
                case 0:
                    version = null;
                    return false;
                case 1:
                    version = new Version(numbs[0], 0, 0, 0);
                    break;
                case 2:
                    version = new Version(numbs[0], numbs[1], 0, 0);
                    break;
                case 3:
                    version = new Version(numbs[0], numbs[1], numbs[2], 0);
                    break;
                case >= 4:
                    version = new Version(numbs[0], numbs[1], numbs[2], numbs[3]);
                    break;
                default: goto case 0;
            }
            return true;
        }

#if HAS_WINRT
        /// <summary>
        /// Try to parse the <see cref="VersionName"/> into a <see cref="PackageVersion"/> object.
        /// </summary>
        /// <param name="version">The <see cref="PackageVersion"/> object.</param>
        /// <returns><see langword="true"/> if the <see cref="VersionName"/> was successfully parsed; otherwise, <see langword="false"/>.</returns>
#if NET
        [SupportedOSPlatform("Windows10.0.10240.0")]
#endif
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public readonly bool TryAsPackageVersion(out PackageVersion version)
        {
            ushort[] numbs = GetVersionNumbers(VersionName).Split('.', StringSplitOptions.RemoveEmptyEntries).Select(ushort.Parse).Take(4).ToArray();
            switch (numbs.Length)
            {
                case 0:
                    version = default;
                    return false;
                case 1:
                    version = new PackageVersion { Major = numbs[0], Minor = 0, Build = 0, Revision = 0 };
                    break;
                case 2:
                    version = new PackageVersion { Major = numbs[0], Minor = numbs[1], Build = 0, Revision = 0 };
                    break;
                case 3:
                    version = new PackageVersion { Major = numbs[0], Minor = numbs[1], Build = numbs[2], Revision = 0 };
                    break;
                case >= 4:
                    version = new PackageVersion { Major = numbs[0], Minor = numbs[1], Build = numbs[2], Revision = numbs[3] };
                    break;
                default: goto case 0;
            }
            return true;
        }
#endif

        /// <summary>
        /// Gets the version numbers from a string.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <returns>The version numbers.</returns>
        private static string GetVersionNumbers(string version)
        {
            string allowedChars = "01234567890.";
            return new string(version.Where(allowedChars.Contains).ToArray());
        }

        /// <inheritdoc/>
        public readonly int Compare([NotNull] object? x, [NotNull] object? y) =>
            x is VersionInfo left && y is VersionInfo right
                ? left.VersionCode.CompareTo(right.VersionCode)
                : throw new NotImplementedException();

        /// <inheritdoc/>
        public readonly int Compare(VersionInfo x, VersionInfo y) => x.VersionCode.CompareTo(y.VersionCode);

        /// <summary>
        /// Deconstruct the <see cref="VersionInfo"/> class.
        /// </summary>
        /// <param name="versionCode">The version code of the application.</param>
        /// <param name="versionName">The version name of the application.</param>
        public readonly void Deconstruct(out int versionCode, out string versionName)
        {
            versionCode = VersionCode;
            versionName = VersionName;
        }

        /// <inheritdoc/>
        public override readonly string ToString() => $"{VersionName} ({VersionCode})";

        /// <summary>
        /// Compares the <see cref="VersionCode"/> of two <see cref="VersionCode"/> values to determine which is greater.
        /// </summary>
        /// <param name="left">The <see cref="VersionCode"/> value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="VersionCode"/> value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="VersionCode"/> of <paramref name="left"/>
        /// is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(VersionInfo left, VersionInfo right) => left.VersionCode > right.VersionCode;

        /// <summary>
        /// Compares the <see cref="VersionCode"/> of two <see cref="VersionCode"/> values to determine which is less.
        /// </summary>
        /// <param name="left">The <see cref="VersionCode"/> value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="VersionCode"/> value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="VersionCode"/> of <paramref name="left"/>
        /// is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(VersionInfo left, VersionInfo right) => left.VersionCode < right.VersionCode;

        /// <summary>
        /// Compares the <see cref="VersionCode"/> of two <see cref="VersionCode"/> values to determine which is greater or equal.
        /// </summary>
        /// <param name="left">The <see cref="VersionCode"/> value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="VersionCode"/> value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="VersionCode"/> of <paramref name="left"/>
        /// is greater or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(VersionInfo left, VersionInfo right) => left.VersionCode >= right.VersionCode;

        /// <summary>
        /// Compares the <see cref="VersionCode"/> of two <see cref="VersionCode"/> values to determine which is less or equal.
        /// </summary>
        /// <param name="left">The <see cref="VersionCode"/> value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The <see cref="VersionCode"/> value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="VersionCode"/> of <paramref name="left"/>
        /// is less or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(VersionInfo left, VersionInfo right) => left.VersionCode <= right.VersionCode;
    }
}
