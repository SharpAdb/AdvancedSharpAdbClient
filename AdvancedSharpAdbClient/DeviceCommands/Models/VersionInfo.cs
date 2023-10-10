// <copyright file="VersionInfo.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents a version of an Android application.
    /// </summary>
    /// <param name="VersionCode">The version code of the application.</param>
    /// <param name="VersionName">The version name of the application.</param>
    public readonly record struct VersionInfo(int VersionCode, string VersionName) : IComparer, IComparer<VersionInfo>
    {
        /// <summary>
        /// Gets or sets the version code of an Android application.
        /// </summary>
        public int VersionCode { get; init; } = VersionCode;

        /// <summary>
        /// Gets or sets the version name of an Android application.
        /// </summary>
        public string VersionName { get; init; } = VersionName;

        /// <inheritdoc/>
        public readonly int Compare(object x, object y) =>
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
