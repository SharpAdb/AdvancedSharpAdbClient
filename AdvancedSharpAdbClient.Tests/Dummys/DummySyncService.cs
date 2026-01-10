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
    internal partial class DummySyncService : ISyncService
    {
        public Dictionary<string, Stream> UploadedFiles { get; } = [];

        public bool IsOpen { get; private set; } = true;

        public void Dispose() => IsOpen = false;

        public void Open() => IsOpen = true;

        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            IsOpen = true;
        }

        public void Pull(string remotePath, Stream stream, Action<SyncProgressChangedEventArgs> callback = null, bool useV2 = false, in bool isCancelled = false)
        {
            for (uint i = 0; i <= 100; i++)
            {
                callback?.Invoke(new SyncProgressChangedEventArgs(i, 100));
            }
        }

        public async Task PullAsync(string remotePath, Stream stream, Action<SyncProgressChangedEventArgs> callback = null, bool useV2 = false, CancellationToken cancellationToken = default)
        {
            for (uint i = 0; i <= 100; i++)
            {
                await Task.Yield();
                callback?.Invoke(new SyncProgressChangedEventArgs(i, 100));
            }
        }

        public void Push(Stream stream, string remotePath, UnixFileStatus permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs> callback = null, bool useV2 = false, in bool isCancelled = false)
        {
            for (uint i = 0; i <= 100; i++)
            {
                if (i == 100)
                {
                    UploadedFiles[remotePath] = stream;
                }
                callback?.Invoke(new SyncProgressChangedEventArgs(i, 100));
            }
        }

        public async Task PushAsync(Stream stream, string remotePath, UnixFileStatus permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs> callback = null, bool useV2 = false, CancellationToken cancellationToken = default)
        {
            for (uint i = 0; i <= 100; i++)
            {
                await Task.Yield();
                if (i == 100)
                {
                    UploadedFiles[remotePath] = stream;
                }
                callback?.Invoke(new SyncProgressChangedEventArgs(i, 100));
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

        IAsyncEnumerable<FileStatisticsEx> ISyncService.GetDirectoryAsyncListingEx(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        IEnumerable<FileStatistics> ISyncService.GetDirectoryListing(string remotePath) => throw new NotImplementedException();

        Task<List<FileStatistics>> ISyncService.GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        IEnumerable<FileStatisticsEx> ISyncService.GetDirectoryListingEx(string remotePath) => throw new NotImplementedException();

        Task<List<FileStatisticsEx>> ISyncService.GetDirectoryListingExAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        FileStatistics ISyncService.Stat(string remotePath) => throw new NotImplementedException();

        Task<FileStatistics> ISyncService.StatAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        FileStatisticsEx ISyncService.StatEx(string remotePath) => throw new NotImplementedException();

        Task<FileStatisticsEx> ISyncService.StatExAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        #endregion
    }
}
