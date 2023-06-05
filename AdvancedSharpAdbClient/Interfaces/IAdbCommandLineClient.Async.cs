#if HAS_TASK
// <copyright file="IAdbCommandLineClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface IAdbCommandLineClient
    {
        /// <summary>
        /// Queries adb for its version number and checks it against <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="Version"/> object that contains the version number of the Android Command Line client.</returns>
        Task<Version> GetVersionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Starts the adb server by running the <c>adb start-server</c> command.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task StartServerAsync(CancellationToken cancellationToken);
    }
}
#endif