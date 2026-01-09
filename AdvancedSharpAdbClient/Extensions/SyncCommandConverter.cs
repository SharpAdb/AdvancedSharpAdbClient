// <copyright file="SyncCommandConverter.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Converts <see cref="SyncCommand"/> values to their binary representation and vice versa.
    /// </summary>
    public static class SyncCommandConverter
    {
        /// <summary>
        /// The extension for the <see cref="SyncCommand"/> enum.
        /// </summary>
        /// <param name="command">The <see cref="SyncCommand"/> to extend.</param>
        extension(SyncCommand command)
        {
            /// <summary>
            /// Gets the byte array that represents the <see cref="SyncCommand"/>.
            /// </summary>
            /// <returns>A byte array that represents the <see cref="SyncCommand"/>.</returns>
            public byte[] GetBytes() => BitConverter.GetBytes((int)command);

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="GetBytes(SyncCommand)"/>.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the <see cref="SyncCommand"/>.</returns>
            public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)command.GetBytes()).GetEnumerator();

            /// <summary>
            /// Determines which <see cref="SyncCommand"/> is represented by this byte array.
            /// </summary>
            /// <param name="value">A byte array that represents a <see cref="SyncCommand"/>.</param>
            /// <returns>The corresponding <see cref="SyncCommand"/>.</returns>
            public static SyncCommand GetCommand(byte[] value)
            {
                ArgumentNullException.ThrowIfNull(value);

                if (value.Length != 4)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                return (SyncCommand)BitConverter.ToInt32(value);
            }

#if HAS_BUFFERS
            /// <summary>
            /// Determines which <see cref="SyncCommand"/> is represented by this byte array.
            /// </summary>
            /// <param name="value">A byte array that represents a <see cref="SyncCommand"/>.</param>
            /// <returns>The corresponding <see cref="SyncCommand"/>.</returns>
            public static SyncCommand GetCommand(ReadOnlySpan<byte> value)
            {
                if (value.Length != 4)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                return (SyncCommand)BitConverter.ToInt32(value);
            }
#endif
        }
    }
}
