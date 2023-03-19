using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DummyAdbSocket : IAdbSocket, IDummyAdbSocket
    {
        /// <summary>
        /// Use this message to cause <see cref="ReadString"/> and <see cref="ReadStringAsync(CancellationToken)"/> to throw
        /// a <see cref="AdbException"/> indicating that the adb server has forcefully closed the connection.
        /// </summary>
        public const string ServerDisconnected = "ServerDisconnected";

        public DummyAdbSocket() => IsConnected = true;

        public Stream ShellStream { get; set; }

        public Queue<AdbResponse> Responses { get; } = new Queue<AdbResponse>();

        public Queue<SyncCommand> SyncResponses { get; } = new Queue<SyncCommand>();

        public Queue<byte[]> SyncDataReceived { get; } = new Queue<byte[]>();

        public Queue<byte[]> SyncDataSent { get; } = new Queue<byte[]>();

        public Queue<string> ResponseMessages { get; } = new Queue<string>();

        public List<string> Requests { get; } = new List<string>();

        public List<(SyncCommand, string)> SyncRequests { get; } = new List<(SyncCommand, string)>();

        public bool IsConnected { get; set; }

        public bool WaitForNewData { get; set; }

        public bool Connected => IsConnected
            && (WaitForNewData || Responses.Count > 0 || ResponseMessages.Count > 0 || SyncResponses.Count > 0 || SyncDataReceived.Count > 0);

        /// <inheritdoc/>
        public bool DidReconnect { get; private set; }

        public Socket Socket => throw new NotImplementedException();

        public void Dispose() => IsConnected = false;

        public int Read(byte[] data)
        {
            byte[] actual = SyncDataReceived.Dequeue();

            for (int i = 0; i < data.Length && i < actual.Length; i++)
            {
                data[i] = actual[i];
            }

            return actual.Length;
        }

        public Task ReadAsync(byte[] data, CancellationToken cancellationToken)
        {
            Read(data);

            return Task.FromResult(true);
        }

        public AdbResponse ReadAdbResponse()
        {
            AdbResponse response = Responses.Dequeue();

            return !response.Okay ? throw new AdbException(response.Message, response) : response;
        }

        public string ReadString() => ReadStringAsync(CancellationToken.None).Result;

        public string ReadSyncString() => ResponseMessages.Dequeue();

        public async Task<string> ReadStringAsync(CancellationToken cancellationToken)
        {
            if (WaitForNewData)
            {
                while (ResponseMessages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            string message = ResponseMessages.Dequeue();

            if (message == ServerDisconnected)
            {
                SocketException socketException = new(AdbServer.ConnectionReset);
                throw new AdbException(socketException.Message, socketException);
            }
            else
            {
                return message;
            }
        }

        public void SendAdbRequest(string request) => Requests.Add(request);

        public void Close() => IsConnected = false;

        public void SendSyncRequest(string command, int value) => throw new NotImplementedException();

        public void Send(byte[] data, int length) => SyncDataSent.Enqueue(data.Take(length).ToArray());

        public int Read(byte[] data, int length)
        {
            byte[] actual = SyncDataReceived.Dequeue();

            Assert.Equal(actual.Length, length);

            Buffer.BlockCopy(actual, 0, data, 0, length);

            return actual.Length;
        }

        public void SendSyncRequest(SyncCommand command, string path) => SyncRequests.Add((command, path));

        public SyncCommand ReadSyncResponse() => SyncResponses.Dequeue();

        public void SendSyncRequest(SyncCommand command, int length) => SyncRequests.Add((command, length.ToString()));

        public void SendSyncRequest(SyncCommand command, string path, int permissions) => SyncRequests.Add((command, $"{path},{permissions}"));

        public Stream GetShellStream()
        {
            if (ShellStream != null)
            {
                return ShellStream;
            }
            else
            {
                // Simulate the device failing to respond properly.
                throw new SocketException();
            }
        }

        public void Reconnect() => DidReconnect = true;

        public Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Send(byte[] data, int offset, int length)
        {
            if (offset == 0)
            {
                Send(data, length);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetDevice(DeviceData device)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                SendAdbRequest($"host:transport:{device.Serial}");

                try
                {
                    AdbResponse response = ReadAdbResponse();
                }
                catch (AdbException e)
                {
                    if (string.Equals("device not found", e.AdbError, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DeviceNotFoundException(device.Serial);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public Task SendAsync(byte[] data, int length)
        {
            Send(data, length);
            return Task.CompletedTask;
        }

        public Task SendAsync(byte[] data, int offset, int length)
        {
            Send(data, offset, length);
            return Task.CompletedTask;
        }

        public Task SendAdbRequestAsync(string request)
        {
            SendAdbRequest(request);
            return Task.CompletedTask;
        }

        public Task<AdbResponse> ReadAdbResponseAsync()
        {
            var response = ReadAdbResponse();
            var tcs = new TaskCompletionSource<AdbResponse>();
            tcs.SetResult(response);
            return tcs.Task;
        }
    }
}
