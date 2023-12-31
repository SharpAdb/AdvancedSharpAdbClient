#if HAS_TASK
// <copyright file="FunctionOutputReceiver.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Threading;

namespace AdvancedSharpAdbClient.Receivers
{
    public readonly partial struct FunctionOutputReceiver : IShellOutputReceiver
    {
        /// <inheritdoc/>
        public Task<bool> AddOutputAsync(string line, CancellationToken cancellationToken = default) =>
            TaskExExtensions.FromResult(predicate(line));

        /// <inheritdoc/>
        public Task FlushAsync(CancellationToken cancellationToken = default) =>
            TaskExExtensions.CompletedTask;
    }
}
#endif