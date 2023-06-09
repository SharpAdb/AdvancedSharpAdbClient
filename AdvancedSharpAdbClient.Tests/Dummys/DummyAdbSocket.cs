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

        public void Send(byte[] data, int length) => SyncDataSent.Enqueue(data.Take(length).ToArray());

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

        public void SendSyncRequest(string command, int value) => SyncRequests.Add((Enum.Parse<SyncCommand>(command), value.ToString()));

        public void SendSyncRequest(SyncCommand command, string path) => SyncRequests.Add((command, path));

        public void SendSyncRequest(SyncCommand command, int length) => SyncRequests.Add((command, length.ToString()));

        public void SendSyncRequest(SyncCommand command, string path, int permissions) => SyncRequests.Add((command, $"{path},{permissions}"));

        public void SendAdbRequest(string request) => Requests.Add(request);

        public int Read(byte[] data)
        {
            byte[] actual = SyncDataReceived.Dequeue();

            for (int i = 0; i < data.Length && i < actual.Length; i++)
            {
                data[i] = actual[i];
            }

            return actual.Length;
        }

        public int Read(byte[] data, int length)
        {
            byte[] actual = SyncDataReceived.Dequeue();

            Assert.Equal(actual.Length, length);

            Buffer.BlockCopy(actual, 0, data, 0, length);

            return actual.Length;
        }

        public string ReadString() => ReadStringAsync(CancellationToken.None).Result;

        public string ReadSyncString() => ResponseMessages.Dequeue();

        public AdbResponse ReadAdbResponse()
        {
            AdbResponse response = Responses.Dequeue();

            return !response.Okay ? throw new AdbException(response.Message, response) : response;
        }

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

        public SyncCommand ReadSyncResponse() => SyncResponses.Dequeue();

        public Task SendAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            Send(data, length);
            return Task.CompletedTask;
        }

        public Task SendAsync(byte[] data, int offset, int length, CancellationToken cancellationToken = default)
        {
            Send(data, offset, length);
            return Task.CompletedTask;
        }

        public Task SendSyncRequestAsync(SyncCommand command, string path, int permissions, CancellationToken cancellationToken = default)
        {
            SendSyncRequest(command, path, permissions);
            return Task.CompletedTask;
        }

        public Task SendSyncRequestAsync(SyncCommand command, string path, CancellationToken cancellationToken = default)
        {
            SendSyncRequest(command, path);
            return Task.CompletedTask;
        }

        public Task SendSyncRequestAsync(SyncCommand command, int length, CancellationToken cancellationToken = default)
        {
            SendSyncRequest(command, length);
            return Task.CompletedTask;
        }

        public Task SendAdbRequestAsync(string request, CancellationToken cancellationToken = default)
        {
            SendAdbRequest(request);
            return Task.CompletedTask;
        }

        public Task<int> ReadAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            int result = Read(data);
            TaskCompletionSource<int> tcs = new();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            int result = Read(data, length);
            TaskCompletionSource<int> tcs = new();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
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

        public Task<string> ReadSyncStringAsync(CancellationToken cancellationToken = default)
        {
            string response = ReadSyncString();
            TaskCompletionSource<string> tcs = new();
            tcs.SetResult(response);
            return tcs.Task;
        }

        public Task<SyncCommand> ReadSyncResponseAsync(CancellationToken cancellationToken = default)
        {
            SyncCommand response = ReadSyncResponse();
            TaskCompletionSource<SyncCommand> tcs = new();
            tcs.SetResult(response);
            return tcs.Task;
        }

        public Task<AdbResponse> ReadAdbResponseAsync(CancellationToken cancellationToken = default)
        {
            AdbResponse response = ReadAdbResponse();
            TaskCompletionSource<AdbResponse> tcs = new();
            tcs.SetResult(response);
            return tcs.Task;
        }

        public Task SetDeviceAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            SetDevice(device);
            return Task.CompletedTask;
        }

        public void Dispose() => IsConnected = false;

        public void Close() => IsConnected = false;

        public void Reconnect() => DidReconnect = true;
    }
}
