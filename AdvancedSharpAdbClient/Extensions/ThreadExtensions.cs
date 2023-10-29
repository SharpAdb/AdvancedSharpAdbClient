#if HAS_TASK
// <copyright file="ThreadExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AdvancedSharpAdbClient
{
#if WINDOWS_UWP
    /// <summary>
    /// A helper type for switch thread by <see cref="CoreDispatcher"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="dispatcher">A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.</param>
    /// <param name="priority">Specifies the priority for event dispatch.</param>
    [Browsable(false)]
    public readonly struct DispatcherThreadSwitcher(CoreDispatcher dispatcher, CoreDispatcherPriority priority) : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted => dispatcher.HasThreadAccess;

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="DispatcherThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public DispatcherThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation();
            }
            else
            {
                _ = dispatcher.RunAsync(priority, () => continuation());
            }
        }
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="DispatcherQueue"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="dispatcher">A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.</param>
    /// <param name="priority">Specifies the priority for event dispatch.</param>
    [Browsable(false)]
    public readonly struct DispatcherQueueThreadSwitcher(DispatcherQueue dispatcher, DispatcherQueuePriority priority) : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted => dispatcher.HasThreadAccess;

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="DispatcherQueueThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public DispatcherQueueThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation();
            }
            else
            {
                _ = dispatcher.TryEnqueue(priority, () => continuation());
            }
        }
    }
#endif

    /// <summary>
    /// A helper type for switch thread by <see cref="Task"/>. This type is not intended to be used directly from your code.
    /// </summary>
    [Browsable(false)]
    public readonly struct TaskThreadSwitcher : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted => SynchronizationContext.Current == null;

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="TaskThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public TaskThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = Extensions.Run(continuation);
    }

    /// <summary>
    /// The extensions for switching threads.
    /// </summary>
    public static class ThreadExtensions
    {
#if WINDOWS_UWP
        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="dispatcher">A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static DispatcherQueueThreadSwitcher ResumeForegroundAsync(this DispatcherQueue dispatcher, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) => new(dispatcher, priority);

        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="dispatcher">A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static DispatcherThreadSwitcher ResumeForegroundAsync(this CoreDispatcher dispatcher, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) => new (dispatcher, priority);
#endif

        /// <summary>
        /// A helper function—for use within a coroutine—that returns control to the caller, and then immediately resumes execution on a thread pool thread.
        /// </summary>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static TaskThreadSwitcher ResumeBackgroundAsync() => new();
    }
}
#endif