using System.Collections.Generic;
using System.IO;

namespace AdvancedSharpAdbClient.Tests
{
    public interface IDummyAdbSocket : IAdbSocket
    {
        Queue<AdbResponse> Responses { get; }

        Queue<string> ResponseMessages { get; }

        List<string> Requests { get; }

        Queue<SyncCommand> SyncResponses { get; }

        Queue<byte[]> SyncDataReceived { get; }

        Queue<byte[]> SyncDataSent { get; }

        List<(SyncCommand, string)> SyncRequests { get; }

        Queue<Stream> ShellStreams { get; }

        /// <summary>
        /// Gets a value indicating whether the socket reconnected.
        /// </summary>
        bool DidReconnect { get; }

        /// <summary>
        /// If <see langword="false"/>, the socket will disconnect as soon as all data has been read. If <see langword="true"/>,
        /// the socket will wait for new messages to appear in the queue.
        /// </summary>
        bool WaitForNewData { get; set; }
    }
}
