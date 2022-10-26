using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    internal class TracingAdbSocket : AdbSocket, IDummyAdbSocket
    {
        public TracingAdbSocket(EndPoint endPoint) : base(endPoint)
        {
        }

        public Stream ShellStream { get; set; }

        public bool DoDispose { get; set; }

        public Queue<AdbResponse> Responses { get; } = new Queue<AdbResponse>();

        public Queue<string> ResponseMessages { get; } = new Queue<string>();

        public Queue<SyncCommand> SyncResponses { get; } = new Queue<SyncCommand>();

        public Queue<byte[]> SyncDataReceived { get; } = new Queue<byte[]>();

        public Queue<byte[]> SyncDataSent { get; } = new Queue<byte[]>();

        public List<string> Requests { get; } = new List<string>();

        public List<Tuple<SyncCommand, string>> SyncRequests { get; } = new List<Tuple<SyncCommand, string>>();

        public bool DidReconnect { get; private set; }

        public bool WaitForNewData { get; set; }

        public override void Dispose()
        {
            if (DoDispose)
            {
                base.Dispose();
            }
        }

        public override int Read(byte[] data)
        {
#if NETCOREAPP1_1
            StackTrace trace = null;
#else
            StackTrace trace = new StackTrace();
#endif

            int read = base.Read(data);

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncDataReceived.Enqueue(data);
            }

            return read;
        }

        public override int Read(byte[] data, int length)
        {
#if NETCOREAPP1_1
            StackTrace trace = null;
#else
            StackTrace trace = new StackTrace();
#endif

            int read = base.Read(data, length);

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncDataReceived.Enqueue(data.Take(length).ToArray());
            }

            return read;
        }

        public override AdbResponse ReadAdbResponse()
        {
            Exception exception = null;
            AdbResponse response;

            try
            {
                response = base.ReadAdbResponse();
            }
            catch (AdbException ex)
            {
                exception = ex;
                response = ex.Response;
            }

            Responses.Enqueue(response);

            return exception != null ? throw exception : response;
        }

        public override string ReadString()
        {
            string value = base.ReadString();
            ResponseMessages.Enqueue(value);
            return value;
        }

        public override string ReadSyncString()
        {
            string value = base.ReadSyncString();
            ResponseMessages.Enqueue(value);
            return value;
        }

        public override async Task<string> ReadStringAsync(CancellationToken cancellationToken)
        {
            string value = await base.ReadStringAsync(cancellationToken);
            ResponseMessages.Enqueue(value);
            return value;
        }

        public override void SendAdbRequest(string request)
        {
            Requests.Add(request);
            base.SendAdbRequest(request);
        }

        public override void SendSyncRequest(SyncCommand command, string path)
        {
            SyncRequests.Add(new Tuple<SyncCommand, string>(command, path));
            base.SendSyncRequest(command, path);
        }

        public override void SendSyncRequest(SyncCommand command, int length)
        {
#if NETCOREAPP1_1
            StackTrace trace = null;
#else
            StackTrace trace = new StackTrace();
#endif

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncRequests.Add(new Tuple<SyncCommand, string>(command, length.ToString()));
            }

            base.SendSyncRequest(command, length);
        }

        public override SyncCommand ReadSyncResponse()
        {
            SyncCommand response = base.ReadSyncResponse();
            SyncResponses.Enqueue(response);
            return response;
        }

        public override void Send(byte[] data, int length)
        {
#if NETCOREAPP1_1
            StackTrace trace = null;
#else
            StackTrace trace = new StackTrace();
#endif

            base.Send(data, length);

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncDataSent.Enqueue(data.Take(length).ToArray());
            }
        }

        public override void Reconnect()
        {
            base.Reconnect();

            DidReconnect = true;
        }
    }
}
