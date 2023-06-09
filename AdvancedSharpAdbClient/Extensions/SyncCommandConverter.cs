// <copyright file="SyncCommandConverter.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Converts <see cref="SyncCommand"/> values to their binary representation and vice versa.
    /// </summary>
    public static class SyncCommandConverter
    {
        /// <summary>
        /// Maps the <see cref="SyncCommand"/> values to their string representations.
        /// </summary>
        private static readonly Dictionary<SyncCommand, string> Values = new()
        {
            { SyncCommand.DATA, "DATA" },
            { SyncCommand.DENT, "DENT" },
            { SyncCommand.DONE, "DONE" },
            { SyncCommand.FAIL, "FAIL" },
            { SyncCommand.LIST, "LIST" },
            { SyncCommand.OKAY, "OKAY" },
            { SyncCommand.RECV, "RECV" },
            { SyncCommand.SEND, "SEND" },
            { SyncCommand.STAT, "STAT" }
        };

        /// <summary>
        /// Gets the byte array that represents the <see cref="SyncCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="SyncCommand"/> to convert.</param>
        /// <returns>A byte array that represents the <see cref="SyncCommand"/>.</returns>
        public static byte[] GetBytes(SyncCommand command)
        {
            if (!Values.ContainsKey(command))
            {
                throw new ArgumentOutOfRangeException(nameof(command), $"{command} is not a valid sync command");
            }

            string commandText = Values[command];
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

            SyncCommand? key = Values.Where(d => string.Equals(d.Value, commandText, StringComparison.OrdinalIgnoreCase)).Select(d => new SyncCommand?(d.Key)).SingleOrDefault();

            return key == null ? throw new ArgumentOutOfRangeException(nameof(value), $"{commandText} is not a valid sync command") : key.Value;
        }
    }
}
