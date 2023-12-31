#if HAS_TASK
// <copyright file="MultiLineReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;

namespace AdvancedSharpAdbClient.Receivers
{
    public abstract partial class MultiLineReceiver
    {
        /// <inheritdoc/>
        public override async Task<bool> AddOutputAsync(string line, CancellationToken cancellationToken = default)
        {
            await ThrowOnErrorAsync(line, cancellationToken).ConfigureAwait(false);
            Lines.Add(TrimLines ? line.Trim() : line);
            return true;
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (Lines.Count > 0)
            {
                // send it for final processing
                await ProcessNewLinesAsync(Lines, cancellationToken).ConfigureAwait(false);
                Lines.Clear();
            }
            await DoneAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously throws an error message if the console output line contains an error message.
        /// </summary>
        /// <param name="line">The line to inspect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected virtual Task ThrowOnErrorAsync(string line, CancellationToken cancellationToken = default)
        {
            ThrowOnError(line);
            return TaskExExtensions.CompletedTask;
        }

        /// <summary>
        /// Asynchronously processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected virtual Task ProcessNewLinesAsync(IEnumerable<string> lines, CancellationToken cancellationToken = default)
        {
            ProcessNewLines(lines);
            return TaskExExtensions.CompletedTask;
        }
    }
}
#endif