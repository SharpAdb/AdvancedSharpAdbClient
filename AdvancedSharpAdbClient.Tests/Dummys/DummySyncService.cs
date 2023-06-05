using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DummySyncService : ISyncService
    {
        public Dictionary<string, Stream> UploadedFiles { get; private set; } = new Dictionary<string, Stream>();

        public bool IsOpen => true;

        public event EventHandler<SyncProgressChangedEventArgs> SyncProgressChanged;

        public void Dispose()
        {
        }

        public IAsyncEnumerable<FileStatistics> GetDirectoryAsyncListing(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerable<FileStatistics> GetDirectoryListing(string remotePath) => throw new NotImplementedException();

        public Task<IEnumerable<FileStatistics>> GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Open()
        {
        }

        public Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void Pull(string remotePath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken = default) =>
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));

        public Task PullAsync(string remotePath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
            return Task.CompletedTask;
        }

        public void Push(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(0, 100));
            UploadedFiles.Add(remotePath, stream);
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
        }

        public Task PushAsync(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(0, 100));
            UploadedFiles.Add(remotePath, stream);
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
            return Task.CompletedTask;
        }

        public FileStatistics Stat(string remotePath) => throw new NotImplementedException();

        public Task<FileStatistics> StatAsync(string remotePath, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
