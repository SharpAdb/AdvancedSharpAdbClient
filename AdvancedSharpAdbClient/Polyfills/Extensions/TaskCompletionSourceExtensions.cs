#if !NET && HAS_TASK
global using TaskCompletionSource = System.Threading.Tasks.TaskCompletionSource<object?>;
using System.Runtime.CompilerServices;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="TaskCompletionSource"/> class.
    /// </summary>
    internal static class TaskCompletionSourceExtensions
    {
        /// <summary>
        /// Attempts to transition the underlying <see cref="Task"/> into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <param name="source">The <see cref="TaskCompletionSource"/> to set the result for.</param>
        /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
        [MethodImpl((MethodImplOptions)0x100)]
        public static bool TrySetResult(this TaskCompletionSource source) => source.TrySetResult(null);
    }
}
#endif