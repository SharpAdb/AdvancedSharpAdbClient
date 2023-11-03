#if HAS_BUFFERS
// <copyright file="EnumerableBuilder.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// A collection builder provide for collection expressions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EnumerableBuilder
    {
        /// <summary>
        /// Build a <see cref="ColorData"/> struct.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="ColorData"/> struct.</param>
        /// <returns>A new instance of <see cref="ColorData"/> struct.</returns>
        public static ColorData ColorDataCreate(ReadOnlySpan<byte> values) =>
            new((uint)(values[0] | (values[1] << 8) | (values[2] << 16) | (values[3] << 24)),
                (uint)(values[4] | (values[5] << 8) | (values[6] << 16) | (values[7] << 24)));

        /// <summary>
        /// Build a <see cref="FramebufferHeader"/> struct.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <returns>A new instance of <see cref="FramebufferHeader"/> struct.</returns>
        public static FramebufferHeader FramebufferHeaderCreate(ReadOnlySpan<byte> values) => new(values);
    }
}
#endif