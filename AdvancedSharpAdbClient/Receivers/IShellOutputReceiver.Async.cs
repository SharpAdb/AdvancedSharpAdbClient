#if HAS_TASK
// <copyright file="IShellOutputReceiver.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Threading;

namespace AdvancedSharpAdbClient.Receivers
{
    public partial interface IShellOutputReceiver
    {
        /// <summary>
        /// Asynchronously adds a line to the output.
        /// </summary>
        /// <param name="line">The line to add to the output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Boolean}"/> which returns <see langword="true"/> if continue receive messages; otherwise <see langword="false"/>.</returns>
        Task<bool> AddOutputAsync(string line, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously flushes the output.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <remarks>This should always be called at the end of the "process" in order to indicate that the data is ready to be processed further if needed.</remarks>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task FlushAsync(CancellationToken cancellationToken);
    }
}
#endif