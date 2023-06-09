// <copyright file="IShellOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// This interface contains various receivers that are able to parse Android console output. You can use
    /// the receivers in combination with the <see cref="AdbClient.ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver)"/>
    /// method to capture the output of any Android command.
    /// </summary>
    public interface IShellOutputReceiver
    {
        /// <summary>
        /// Gets a value indicating whether the receiver parses error messages.
        /// </summary>
        /// <value><see langword="true"/> if this receiver parsers error messages; otherwise <see langword="false"/>.</value>
        /// <remarks>The default value is <see langword="false"/>. If set to <see langword="false"/>, the <see cref="AdbClient"/>
        /// will detect common error messages and throw an exception.</remarks>
        bool ParsesErrors { get; }

        /// <summary>
        /// Adds the output.
        /// </summary>
        void AddOutput(string line);

        /// <summary>
        /// Flushes the output.
        /// </summary>
        /// <remarks>This should always be called at the end of the "process" in order to indicate that the data is ready to be processed further if needed.</remarks>
        void Flush();
    }
}
