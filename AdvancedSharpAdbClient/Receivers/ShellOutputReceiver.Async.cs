#if HAS_TASK
// <copyright file="ShellOutputReceiver.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Threading;

namespace AdvancedSharpAdbClient.Receivers
{
    public abstract partial class ShellOutputReceiver
    {
        /// <inheritdoc/>
        public virtual Task<bool> AddOutputAsync(string line, CancellationToken cancellationToken = default) =>
            TaskExExtensions.FromResult(AddOutput(line));

        /// <inheritdoc/>
        public virtual Task FlushAsync(CancellationToken cancellationToken = default) =>
            DoneAsync(cancellationToken);

        /// <summary>
        /// Finishes the receiver asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected virtual Task DoneAsync(CancellationToken cancellationToken = default)
        {
            Done();
            return TaskExExtensions.CompletedTask;
        }
    }
}
#endif