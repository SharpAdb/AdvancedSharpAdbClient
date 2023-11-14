using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Locker for the <see cref="Factories"/> class.
    /// </summary>
    public class FactoriesLocker : IDisposable
    {
        public static SemaphoreSlim SlimLocker { get; } = new(1, 1);

        public static FactoriesLocker Wait()
        {
            SlimLocker.Wait();
            return new FactoriesLocker();
        }

        public static async Task<FactoriesLocker> WaitAsync()
        {
            await SlimLocker.WaitAsync();
            return new FactoriesLocker();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SlimLocker.Release();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
