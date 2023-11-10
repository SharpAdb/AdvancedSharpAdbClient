using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// A mock implementation of the <see cref="ISyncService"/> class.
    /// </summary>
    internal class DummySyncService : ISyncService
    {
        public Dictionary<string, Stream> UploadedFiles { get; } = [];

        public bool IsOpen { get; private set; } = true;

        public event EventHandler<SyncProgressChangedEventArgs> SyncProgressChanged;

        public void Dispose() => IsOpen = false;

        public void Open() => IsOpen = true;

        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            IsOpen = true;
        }

        public void Pull(string remotePath, Stream stream, IProgress<int> progress = null, in bool isCancelled = false)
        {
            for (int i = 0; i <= 100; i++)
            {
                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(i, 100));
            }
        }

        public async Task PullAsync(string remotePath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            for (int i = 0; i <= 100; i++)
            {
                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(i, 100));
            }
        }

        public void Push(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress = null, in bool isCancelled = false)
        {
            for (int i = 0; i <= 100; i++)
            {
                if (i == 100)
                {
                    UploadedFiles[remotePath] = stream;
                }
                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(i, 100));
            }
        }

        public async Task PushAsync(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            for (int i = 0; i <= 100; i++)
            {
                if (i == 100)
                {
                    UploadedFiles[remotePath] = stream;
                }
                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(i, 100));
            }
        }

        public void Reopen() => IsOpen = true;

        public async Task ReopenAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            IsOpen = true;
        }

        #region Not Implemented

        IAsyncEnumerable<FileStatistics> ISyncService.GetDirectoryAsyncListing(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        IEnumerable<FileStatistics> ISyncService.GetDirectoryListing(string remotePath) => throw new NotImplementedException();

        Task<List<FileStatistics>> ISyncService.GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        FileStatistics ISyncService.Stat(string remotePath) => throw new NotImplementedException();

        Task<FileStatistics> ISyncService.StatAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        #endregion
    }
}
