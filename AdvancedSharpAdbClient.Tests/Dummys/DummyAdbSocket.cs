using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// A mock implementation of the <see cref="IAdbSocket"/> class.
    /// </summary>
    internal class DummyAdbSocket : IDummyAdbSocket, ICloneable<DummyAdbSocket>
    {
        /// <summary>
        /// Use this message to cause <see cref="ReadString"/> and <see cref="ReadStringAsync(CancellationToken)"/> to throw
        /// a <see cref="AdbException"/> indicating that the adb server has forcefully closed the connection.
        /// </summary>
        public const string ServerDisconnected = "ServerDisconnected";

        public DummyAdbSocket() => IsConnected = true;

        public Queue<AdbResponse> Responses { get; init; } = new Queue<AdbResponse>();

        public Queue<SyncCommand> SyncResponses { get; init; } = new Queue<SyncCommand>();

        public Queue<byte[]> SyncDataReceived { get; init; } = new Queue<byte[]>();

        public Queue<byte[]> SyncDataSent { get; init; } = new Queue<byte[]>();

        public Queue<string> ResponseMessages { get; init; } = new Queue<string>();

        public List<string> Requests { get; init; } = [];

        public List<(SyncCommand, string)> SyncRequests { get; init; } = [];

        public Queue<Stream> ShellStreams { get; init; } = new Queue<Stream>();

        public bool IsConnected { get; set; }

        public bool WaitForNewData { get; set; }

        public bool Connected => IsConnected
            && (WaitForNewData || Responses.Count > 0 || ResponseMessages.Count > 0 || SyncResponses.Count > 0 || SyncDataReceived.Count > 0);

        /// <inheritdoc/>
        public bool DidReconnect { get; private set; }

        public void Send(byte[] data) => SyncDataSent.Enqueue(data);

        public void Send(byte[] data, int length) => SyncDataSent.Enqueue(data[..length]);

        public void Send(byte[] data, int offset, int length) => SyncDataSent.Enqueue(data.AsSpan(offset, length).ToArray());

        public void Send(ReadOnlySpan<byte> data) => SyncDataSent.Enqueue(data.ToArray());

        public void SendSyncRequest(string command, int value) => SyncRequests.Add((Enum.Parse<SyncCommand>(command), value.ToString()));

        public void SendSyncRequest(SyncCommand command, string path) => SyncRequests.Add((command, path));

        public void SendSyncRequest(SyncCommand command, int length) => SyncRequests.Add((command, length.ToString()));

        public void SendSyncRequest(SyncCommand command, string path, UnixFileStatus permission) => SyncRequests.Add((command, $"{path},{(int)permission}"));

        public void SendAdbRequest(string request) => Requests.Add(request);

        public int Read(byte[] data)
        {
            Span<byte> actual = SyncDataReceived.Dequeue();
            Assert.True(actual[..Math.Min(actual.Length, data.Length)].TryCopyTo(data));
            return actual.Length;
        }

        public int Read(byte[] data, int length)
        {
            Span<byte> actual = SyncDataReceived.Dequeue();
            Assert.True(actual.Length >= length);
            Assert.True(actual[..length].TryCopyTo(data));
            return length;
        }

        public int Read(byte[] data, int offset, int length)
        {
            Span<byte> actual = SyncDataReceived.Dequeue();
            Assert.True(actual.Length >= length);
            Assert.True(actual[..length].TryCopyTo(data.AsSpan(offset)));
            return length;
        }

        public int Read(Span<byte> data)
        {
            Span<byte> actual = SyncDataReceived.Dequeue();
            Assert.True(actual[..Math.Min(actual.Length, data.Length)].TryCopyTo(data));
            return actual.Length;
        }

        public string ReadString()
        {
            if (WaitForNewData)
            {
                while (ResponseMessages.Count == 0)
                {
                    Thread.Sleep(100);
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

        public string ReadSyncString() => ResponseMessages.Dequeue();

        public AdbResponse ReadAdbResponse()
        {
            AdbResponse response = Responses.Dequeue();

            return !response.Okay ? throw new AdbException(response.Message, response) : response;
        }

        public Stream GetShellStream()
        {
            if (ShellStreams.Dequeue() is Stream actual)
            {
                return actual;
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
                if (uint.TryParse(device.TransportId, out uint tid))
                {
                    SendAdbRequest($"host:transport-id:{tid}");
                }
                else
                {
                    SendAdbRequest($"host:transport:{device.Serial}");
                }

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

        public async Task SendAsync(byte[] data, CancellationToken cancellationToken)
        {
            await Task.Yield();
            Send(data);
        }

        public async Task SendAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            Send(data, length);
        }

        public async Task SendAsync(byte[] data, int offset, int length, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            Send(data, offset, length);
        }

        public async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            Send(data.Span);
        }

        public async Task SendSyncRequestAsync(SyncCommand command, string path, UnixFileStatus permission, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            SendSyncRequest(command, path, permission);
        }

        public async Task SendSyncRequestAsync(SyncCommand command, string path, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            SendSyncRequest(command, path);
        }

        public async Task SendSyncRequestAsync(SyncCommand command, int length, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            SendSyncRequest(command, length);
        }

        public async Task SendAdbRequestAsync(string request, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            SendAdbRequest(request);
        }

        public async Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return Read(data, length);
        }

        public async ValueTask<int> ReadAsync(Memory<byte> data, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return Read(data.Span);
        }

        public async Task<int> ReadAsync(byte[] data, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return Read(data);
        }

        public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            if (WaitForNewData)
            {
                while (ResponseMessages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
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

        public async Task<string> ReadSyncStringAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ReadSyncString();
        }

        public async Task<SyncCommand> ReadSyncResponseAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ReadSyncResponse();
        }

        public async Task<AdbResponse> ReadAdbResponseAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ReadAdbResponse();
        }

        public async Task SetDeviceAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            SetDevice(device);
        }

        public void Dispose() => IsConnected = false;

        public void Close() => IsConnected = false;

        public void Reconnect(bool isForce = false) => DidReconnect = true;

        public async Task ReconnectAsync(bool isForce, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            DidReconnect = true;
        }

        public DummyAdbSocket Clone() => new()
        {
            Responses = Responses,
            SyncResponses = SyncResponses,
            SyncDataReceived = SyncDataReceived,
            SyncDataSent = SyncDataSent,
            ResponseMessages = ResponseMessages,
            Requests = Requests,
            SyncRequests = SyncRequests,
            ShellStreams = ShellStreams
        };

        object ICloneable.Clone() => Clone();
    }
}
