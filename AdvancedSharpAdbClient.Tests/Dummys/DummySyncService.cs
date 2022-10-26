using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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

        public IEnumerable<FileStatistics> GetDirectoryListing(string remotePath) => throw new NotImplementedException();

        public void Open()
        {
        }

        public void Pull(string remotePath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
        }

        public void Push(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken)
        {
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(0, 100));
            UploadedFiles.Add(remotePath, stream);
            SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(100, 100));
        }

        public FileStatistics Stat(string remotePath) => throw new NotImplementedException();
    }
}
