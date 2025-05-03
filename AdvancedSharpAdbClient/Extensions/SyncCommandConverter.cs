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
        extension(SyncCommand command)
        {
            /// <summary>
            /// Gets the byte array that represents the <see cref="SyncCommand"/>.
            /// </summary>
            /// <returns>A byte array that represents the <see cref="SyncCommand"/>.</returns>
            public byte[] GetBytes()
            {
                if (command == 0)
                {
                    return [0, 0, 0, 0];
                }

                if (command is < SyncCommand.STAT or > SyncCommand.LST2)
                {
                    throw new ArgumentOutOfRangeException(nameof(command), $"{command} is not a valid sync command");
                }

                string commandText = command.ToString();
                byte[] commandBytes = AdbClient.Encoding.GetBytes(commandText);

                return commandBytes;
            }

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

                string commandText = AdbClient.Encoding.GetString(value);
                return commandText == "\0\0\0\0" ? 0 : Enum.TryParse(commandText, true, out SyncCommand result) ? result : throw new ArgumentOutOfRangeException(nameof(value), $"{commandText} is not a valid sync command");
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

                string commandText = AdbClient.Encoding.GetString(value);
                return commandText == "\0\0\0\0" ? 0 : Enum.TryParse(commandText, true, out SyncCommand result) ? result : throw new ArgumentOutOfRangeException(nameof(value), $"{commandText} is not a valid sync command");
            }
#endif
        }
    }
}
