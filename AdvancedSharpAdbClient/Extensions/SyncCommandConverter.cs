// <copyright file="SyncCommandConverter.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Converts <see cref="SyncCommand"/> values to their binary representation and vice versa.
    /// </summary>
    public static class SyncCommandConverter
    {
        /// <summary>
        /// Gets the byte array that represents the <see cref="SyncCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="SyncCommand"/> to convert.</param>
        /// <returns>A byte array that represents the <see cref="SyncCommand"/>.</returns>
        public static byte[] GetBytes(SyncCommand command)
        {
            if (command == 0)
            {
                return [0, 0, 0, 0];
            }

            if (command is not (SyncCommand.LIST
                or SyncCommand.RECV
                or SyncCommand.SEND
                or SyncCommand.STAT
                or SyncCommand.DENT
                or SyncCommand.FAIL
                or SyncCommand.DATA
                or SyncCommand.OKAY
                or SyncCommand.DONE))
            {
                throw new ArgumentOutOfRangeException(nameof(command), $"{command} is not a valid sync command");
            }

            string commandText = command.ToString();
            byte[] commandBytes = AdbClient.Encoding.GetBytes(commandText);

            return commandBytes;
        }

        /// <summary>
        /// Determines which <see cref="SyncCommand"/> is represented by this byte array.
        /// </summary>
        /// <param name="value">A byte array that represents a <see cref="SyncCommand"/>.</param>
        /// <returns>The corresponding <see cref="SyncCommand"/>.</returns>
        public static SyncCommand GetCommand(byte[] value)
        {
            ExceptionExtensions.ThrowIfNull(value);

            if (value.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            string commandText = AdbClient.Encoding.GetString(value);
            return commandText == "\0\0\0\0" ? 0 : EnumExtensions.TryParse(commandText, true, out SyncCommand command) ? command : throw new ArgumentOutOfRangeException(nameof(value), $"{commandText} is not a valid sync command");
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
            return commandText == "\0\0\0\0" ? 0 : EnumExtensions.TryParse(commandText, true, out SyncCommand command) ? command : throw new ArgumentOutOfRangeException(nameof(value), $"{commandText} is not a valid sync command");
        }
#endif
    }
}
