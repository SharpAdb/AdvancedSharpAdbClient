// <copyright file="IShellOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Text;

namespace AdvancedSharpAdbClient.Receivers
{
    /// <summary>
    /// This interface contains various receivers that are able to parse Android console output. You can use
    /// the receivers in combination with the <see cref="AdbClient.ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver, Encoding)"/>
    /// method to capture the output of any Android command.
    /// </summary>
    public partial interface IShellOutputReceiver
    {
        /// <summary>
        /// Adds a line to the output.
        /// </summary>
        /// <param name="line">The line to add to the output.</param>
        /// <returns><see langword="true"/> if continue receive messages; otherwise <see langword="false"/>.</returns>
        bool AddOutput(string line);

        /// <summary>
        /// Flushes the output.
        /// </summary>
        /// <remarks>This should always be called at the end of the "process" in order to indicate that the data is ready to be processed further if needed.</remarks>
        void Flush();
    }
}
