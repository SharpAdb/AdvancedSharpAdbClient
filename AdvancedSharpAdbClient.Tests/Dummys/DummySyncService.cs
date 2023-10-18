using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DummySyncService : ISyncService
    {
        public Dictionary<string, Stream> UploadedFiles { get; } = [];

        public bool IsOpen { get; private set; } = true;

        public event EventHandler<SyncProgressChangedEventArgs> SyncProgressChanged;

        public void Dispose() => IsOpen = false;

        public void Open() => IsOpen = true;

        public Task OpenAsync(CancellationToken cancellationToken)
        {
            IsOpen = true;
            return Task.CompletedTask;
        }

        public void Pull(string remotePath, Stream stream, IProgress<int> progress = null, in bool isCancelled = false) =>
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));

        public Task PullAsync(string remotePath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
            return Task.CompletedTask;
        }

        public void Push(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress = null, in bool isCancelled = false)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(0, 100));
            UploadedFiles[remotePath] = stream;
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
        }

        public Task PushAsync(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(0, 100));
            UploadedFiles[remotePath] = stream;
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
            return Task.CompletedTask;
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
