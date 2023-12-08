#if HAS_TASK
// <copyright file="IDeviceMonitor.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface IDeviceMonitor
    {
        /// <summary>
        /// Asynchronously starts the monitoring.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous dispose operation.</returns>
        Task DisposeAsync();
    }
}
#endif