#if !HAS_BUFFERS
// <copyright file="BitConverterExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Runtime.CompilerServices;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="BitConverter"/> class.
    /// </summary>
    internal static class BitConverterExtensions
    {
        /// <summary>
        /// The extension for the <see cref="BitConverter"/> class.
        /// </summary>
        extension(BitConverter)
        {
            /// <summary>
            /// Returns a 32-bit signed integer converted from four bytes in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes that includes the four bytes to convert.</param>
            /// <returns>A 32-bit signed integer representing the converted bytes.</returns>
            [MethodImpl((MethodImplOptions)0x100)]
            public static int ToInt32(byte[] value) => BitConverter.ToInt32(value, 0);

            /// <summary>
            /// Returns a 16-bit unsigned integer converted from two bytes in a byte array.
            /// </summary>
            /// <param name="value">The array of bytes that includes the two bytes to convert.</param>
            /// <returns>An 16-bit unsigned integer representing the converted bytes.</returns>
            [MethodImpl((MethodImplOptions)0x100)]
            public static ushort ToUInt16(byte[] value) => BitConverter.ToUInt16(value, 0);

            /// <summary>
            /// Returns a 32-bit unsigned integer converted from four bytes in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes.</param>
            /// <returns>A 32-bit unsigned integer representing the converted bytes.</returns>
            [MethodImpl((MethodImplOptions)0x100)]
            public static uint ToUInt32(byte[] value) => BitConverter.ToUInt32(value, 0);
        }
    }
}
#endif