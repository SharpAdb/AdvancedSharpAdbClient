using AdvancedSharpAdbClient.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Xunit;

namespace AdvancedSharpAdbClient
{
    public class SocketBasedTests
    {
        protected AdbClient TestClient { get; private set; }

        protected SocketBasedTests(bool integrationTest, bool doDispose)
        {
            // this.EndPoint = AdbClient.Instance.EndPoint;

#if DEBUG
            // Use the tracing adb socket factory to run the tests on an actual device.
            // use the dummy socket factory to run unit tests.
            if (integrationTest)
            {
                TracingAdbSocket tracingSocket = new(EndPoint) { DoDispose = doDispose };

                Factories.AdbSocketFactory = (endPoint) => tracingSocket;
            }
            else
            {
                DummyAdbSocket socket = new();
                Factories.AdbSocketFactory = (endPoint) => socket;
            }

            IntegrationTest = integrationTest;
#else
            // In release mode (e.g. on the build server),
            // never run integration tests.
            DummyAdbSocket socket = new DummyAdbSocket();
            Factories.AdbSocketFactory = (endPoint) => socket;
            IntegrationTest = false;
#endif
            Socket = (IDummyAdbSocket)Factories.AdbSocketFactory(EndPoint);

            TestClient = new AdbClient();
        }

        protected static AdbResponse[] NoResponses { get; } = Array.Empty<AdbResponse>();
        protected static AdbResponse[] OkResponse { get; } = new AdbResponse[] { AdbResponse.OK };
        protected static string[] NoResponseMessages { get; } = Array.Empty<string>();
        protected static DeviceData Device { get; } = new()
        {
            Serial = "169.254.109.177:5555",
            State = DeviceState.Online
        };

        protected IDummyAdbSocket Socket { get; set; }

        public EndPoint EndPoint { get; set; }

        public bool IntegrationTest { get; set; }

        /// <summary>
        /// <para>
        /// Runs an ADB helper test, either as a unit test or as an integration test.
        /// </para>
        /// <para>
        /// When running as a unit test, the <paramref name="responses"/> and <paramref name="responseMessages"/>
        /// are used by the <see cref="DummyAdbSocket"/> to mock the responses an actual device
        /// would send; and the <paramref name="requests"/> parameter is used to ensure the code
        /// did send the correct requests to the device.
        /// </para>
        /// <para>
        /// When running as an integration test, all three parameters, <paramref name="responses"/>,
        /// <paramref name="responseMessages"/> and <paramref name="requests"/> are used to validate
        /// that the traffic we simulate in the unit tests matches the trafic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">
        /// The <see cref="AdbResponse"/> messages that the ADB sever should send.
        /// </param>
        /// <param name="responseMessages">
        /// The messages that should follow the <paramref name="responses"/>.
        /// </param>
        /// <param name="requests">
        /// The requests the client should send.
        /// </param>
        /// <param name="test">
        /// The test to run.
        /// </param>
        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Action test) =>
            RunTest(responses, responseMessages, requests, null, null, null, null, null, test);

        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Stream shellStream,
            Action test) =>
            RunTest(responses, responseMessages, requests, null, null, null, null, shellStream, test);

        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            Action test) =>
            RunTest(
                responses,
                responseMessages,
                requests,
                syncRequests,
                syncResponses,
                syncDataReceived,
                syncDataSent,
                null,
                test);

        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            Stream shellStream,
            Action test)
        {
            // If we are running unit tests, we need to mock all the responses
            // that are sent by the device. Do that now.
            if (!IntegrationTest)
            {
                Socket.ShellStream = shellStream;

                foreach (AdbResponse response in responses)
                {
                    Socket.Responses.Enqueue(response);
                }

                foreach (string responseMessage in responseMessages)
                {
                    Socket.ResponseMessages.Enqueue(responseMessage);
                }

                if (syncResponses != null)
                {
                    foreach (SyncCommand syncResponse in syncResponses)
                    {
                        Socket.SyncResponses.Enqueue(syncResponse);
                    }
                }

                if (syncDataReceived != null)
                {
                    foreach (byte[] syncDatum in syncDataReceived)
                    {
                        Socket.SyncDataReceived.Enqueue(syncDatum);
                    }
                }
            }

            Exception exception = null;

            try
            {
                test();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (!IntegrationTest)
            {
                // If we are running unit tests, we need to make sure all messages
                // were read, and the correct request was sent.

                // Make sure the messages were read
                Assert.Empty(Socket.ResponseMessages);
                Assert.Empty(Socket.Responses);
                Assert.Empty(Socket.SyncResponses);
                Assert.Empty(Socket.SyncDataReceived);

                // Make sure a request was sent
                Assert.Equal(requests.ToList(), Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests.ToList(), Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                if (syncDataSent != null)
                {
                    AssertEqual(syncDataSent.ToList(), Socket.SyncDataSent.ToList());
                }
                else
                {
                    Assert.Empty(Socket.SyncDataSent);
                }
            }
            else
            {
                // Make sure the traffic sent on the wire matches the traffic
                // we have defined in our unit test.
                Assert.Equal(requests.ToList(), Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests.ToList(), Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                Assert.Equal(responses.ToList(), Socket.Responses);
                Assert.Equal(responseMessages.ToList(), Socket.ResponseMessages);

                if (syncResponses != null)
                {
                    Assert.Equal(syncResponses.ToList(), Socket.SyncResponses);
                }
                else
                {
                    Assert.Empty(Socket.SyncResponses);
                }

                if (syncDataReceived != null)
                {
                    AssertEqual(syncDataReceived.ToList(), Socket.SyncDataReceived.ToList());
                }
                else
                {
                    Assert.Empty(Socket.SyncDataReceived);
                }

                if (syncDataSent != null)
                {
                    AssertEqual(syncDataSent.ToList(), Socket.SyncDataSent.ToList());
                }
                else
                {
                    Assert.Empty(Socket.SyncDataSent);
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        protected static IEnumerable<string> Requests(params string[] requests) => requests;

        protected static IEnumerable<string> ResponseMessages(params string[] requests) => requests;

        protected static IEnumerable<(SyncCommand, string)> SyncRequests(SyncCommand command, string path)
        {
            yield return (command, path);
        }

        protected static IEnumerable<(SyncCommand, string)> SyncRequests(SyncCommand command, string path, SyncCommand command2, string path2)
        {
            yield return (command, path);
            yield return (command2, path2);
        }

        protected static IEnumerable<(SyncCommand, string)> SyncRequests(SyncCommand command, string path, SyncCommand command2, string path2, SyncCommand command3, string path3)
        {
            yield return (command, path);
            yield return (command2, path2);
            yield return (command3, path3);
        }

        protected static IEnumerable<AdbResponse> OkResponses(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return AdbResponse.OK;
            }
        }

        private static void AssertEqual(IList<byte[]> expected, IList<byte[]> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
    }
}
