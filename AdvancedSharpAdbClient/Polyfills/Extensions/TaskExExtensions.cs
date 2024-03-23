#if HAS_TASK
// <copyright file="TaskExExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

#if NET40
using Microsoft.Runtime.CompilerServices;
#endif

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="Task"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TaskExExtensions
    {
#if NETFRAMEWORK && !NET46_OR_GREATER
        /// <summary>
        /// Singleton cached task that's been completed successfully.
        /// </summary>
        internal static readonly Task s_cachedCompleted =
#if NET45_OR_GREATER
            Task.
#else
            TaskEx.
#endif
            FromResult<object?>(null);

        /// <summary>
        /// Gets a task that's already been completed successfully.
        /// </summary>
        public static Task CompletedTask => s_cachedCompleted;
#else
        /// <summary>
        /// Gets a task that's already been completed successfully.
        /// </summary>
        public static Task CompletedTask => Task.CompletedTask;
#endif

        /// <summary>
        /// Creates a task that completes after a specified number of milliseconds.
        /// </summary>
        /// <param name="dueTime">The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the time delay.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> argument is less than -1.</exception>
        public static Task Delay(int dueTime, CancellationToken cancellationToken = default) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .Delay(dueTime, cancellationToken);

        /// <summary>
        /// Creates a task that will complete when all of the <see cref="Task"/> objects in an enumerable collection have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        public static Task WhenAll(this IEnumerable<Task> tasks) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .WhenAll(tasks);

        /// <summary>
        /// Creates a task that will complete when all of the <see cref="Task{TResult}"/> objects in an enumerable collection have completed.
        /// </summary>
        /// <typeparam name="TResult">The type of the completed task.</typeparam>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .WhenAll(tasks);

        /// <summary>
        /// Creates a <see cref="Task{TResult}"/> that's completed successfully with the specified result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="result">The result to store into the completed task.</param>
        /// <returns>The successfully completed task.</returns>
        public static Task<TResult> FromResult<TResult>(TResult result) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .FromResult(result);

#if WINDOWS_UWP
        /// <summary>
        /// Wait a <see cref="Task{TResult}"/> synchronously by <see cref="TaskCompletionSource{TResult}"/> and return the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="function">The <see cref="Task{TResult}"/> to wait.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        /// <returns>The result of the completed task.</returns>
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public static TResult AwaitByTaskCompleteSource<TResult>(this IAsyncOperation<TResult> function, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    TResult result = await function.AsTask(cancellationToken).ConfigureAwait(false);
                    _ = taskCompletionSource.TrySetResult(result);
                }
                catch (Exception e)
                {
                    _ = taskCompletionSource.TrySetException(e);
                }
            }, cancellationToken);
            TResult taskResult = task.Result;
            return taskResult;
        }
#endif

        /// <summary>
        /// Wait a <see cref="Task"/> synchronously by <see cref="TaskCompletionSource{Object}"/>.
        /// </summary>
        /// <param name="function">The <see cref="Task"/> to wait.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        public static void AwaitByTaskCompleteSource(this Task function, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<object?> taskCompletionSource = new();
            Task<object?> task = taskCompletionSource.Task;
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await function.ConfigureAwait(false);
                    _ = taskCompletionSource.TrySetResult(null);
                }
                catch (Exception e)
                {
                    _ = taskCompletionSource.TrySetException(e);
                }
            }, cancellationToken);
            _ = task.Result;
        }

        /// <summary>
        /// Wait a <see cref="Task{TResult}"/> synchronously by <see cref="TaskCompletionSource{Object}"/> and return the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="function">The <see cref="Task{TResult}"/> to wait.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        /// <returns>The result of the completed task.</returns>
        public static TResult AwaitByTaskCompleteSource<TResult>(this Task<TResult> function, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    TResult result = await function.ConfigureAwait(false);
                    _ = taskCompletionSource.TrySetResult(result);
                }
                catch (Exception e)
                {
                    _ = taskCompletionSource.TrySetException(e);
                }
            }, cancellationToken);
            TResult taskResult = task.Result;
            return taskResult;
        }
    }
}
#endif