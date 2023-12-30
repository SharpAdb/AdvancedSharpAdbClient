using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public class SocketBasedTests
    {
        protected SocketBasedTests(bool integrationTest, bool doDispose)
        {
            Func<EndPoint, IAdbSocket> AdbSocketFactory;

            // this.EndPoint = AdbClient.Instance.EndPoint;
#if DEBUG
            // Use the tracing adb socket factory to run the tests on an actual device.
            // use the dummy socket factory to run unit tests.
            if (integrationTest)
            {
                TracingAdbSocket tracingSocket = new(EndPoint) { DoDispose = doDispose };
                AdbSocketFactory = endPoint => tracingSocket;
            }
            else
            {
                DummyAdbSocket socket = new();
                AdbSocketFactory = endPoint => socket;
            }

            IntegrationTest = integrationTest;
#else
            // In release mode (e.g. on the build server),
            // never run integration tests.
            DummyAdbSocket socket = new();
            AdbSocketFactory = endPoint => socket;
            IntegrationTest = false;
#endif
            Socket = (IDummyAdbSocket)AdbSocketFactory(EndPoint);

            TestClient = new AdbClient(AdbSocketFactory);
        }

        protected static AdbResponse[] NoResponses { get; } = [];
        protected static AdbResponse[] OkResponse { get; } = [AdbResponse.OK];
        protected static string[] NoResponseMessages { get; } = [];
        protected static string[] NoRequests { get; } = [];
        protected static (SyncCommand, string)[] NoSyncRequests { get; } = [];
        protected static SyncCommand[] NoSyncResponses { get; } = [];
        protected static DeviceData Device { get; } = new()
        {
            Serial = "169.254.109.177:5555",
            State = DeviceState.Online
        };

        protected AdbClient TestClient { get; init; }

        protected IDummyAdbSocket Socket { get; set; }

        public EndPoint EndPoint { get; set; }

        public bool IntegrationTest { get; set; }

        #region Action

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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="test">The test to run.</param>
        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Action test) =>
            RunTest(responses, responseMessages, requests, null, null, null, null, null, test);

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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<Stream> shellStreams,
            Action test) =>
            RunTest(responses, responseMessages, requests, null, null, null, null, shellStreams, test);

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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="test">The test to run.</param>
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        protected void RunTest(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            IEnumerable<Stream> shellStreams,
            Action test)
        {
            // If we are running unit tests, we need to mock all the responses
            // that are sent by the device. Do that now.
            if (!IntegrationTest)
            {
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

                if (shellStreams != null)
                {
                    foreach (Stream shellStream in shellStreams)
                    {
                        Socket.ShellStreams.Enqueue(shellStream);
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
                Assert.Empty(Socket.ShellStreams);

                // Make sure a request was sent
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
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
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                Assert.Equal(responses, Socket.Responses);
                Assert.Equal(responseMessages, Socket.ResponseMessages);

                if (syncResponses != null)
                {
                    Assert.Equal(syncResponses, Socket.SyncResponses);
                }
                else
                {
                    Assert.Empty(Socket.SyncResponses);
                }

                if (syncDataReceived != null)
                {
                    Assert.Equal(syncDataReceived, Socket.SyncDataReceived);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataReceived);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataSent);
                }

                if (shellStreams != null)
                {
                    Assert.Equal(shellStreams, Socket.ShellStreams);
                }
                else
                {
                    Assert.Empty(Socket.ShellStreams);
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        #endregion

        #region Function

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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>The result of <paramref name="test"/>.</returns>
        protected TResult RunTest<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Func<TResult> test) =>
            RunTest(responses, responseMessages, requests, null, null, null, null, null, test);

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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>The result of <paramref name="test"/>.</returns>
        protected TResult RunTest<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<Stream> shellStreams,
            Func<TResult> test) =>
            RunTest(responses, responseMessages, requests, null, null, null, null, shellStreams, test);

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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>The result of <paramref name="test"/>.</returns>
        protected TResult RunTest<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            Func<TResult> test) =>
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>The result of <paramref name="test"/>.</returns>
        protected TResult RunTest<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            IEnumerable<Stream> shellStreams,
            Func<TResult> test)
        {
            // If we are running unit tests, we need to mock all the responses
            // that are sent by the device. Do that now.
            if (!IntegrationTest)
            {
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

                if (shellStreams != null)
                {
                    foreach (Stream shellStream in shellStreams)
                    {
                        Socket.ShellStreams.Enqueue(shellStream);
                    }
                }
            }

            TResult result = default;
            Exception exception = null;

            try
            {
                result = test();
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
                Assert.Empty(Socket.ShellStreams);

                // Make sure a request was sent
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
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
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                Assert.Equal(responses, Socket.Responses);
                Assert.Equal(responseMessages, Socket.ResponseMessages);

                if (syncResponses != null)
                {
                    Assert.Equal(syncResponses, Socket.SyncResponses);
                }
                else
                {
                    Assert.Empty(Socket.SyncResponses);
                }

                if (syncDataReceived != null)
                {
                    Assert.Equal(syncDataReceived, Socket.SyncDataReceived);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataReceived);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataSent);
                }

                if (shellStreams != null)
                {
                    Assert.Equal(shellStreams, Socket.ShellStreams);
                }
                else
                {
                    Assert.Empty(Socket.ShellStreams);
                }
            }

            return exception != null ? throw exception : result;
        }

        #endregion

        #region Task

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected Task RunTestAsync(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Func<Task> test) =>
            RunTestAsync(responses, responseMessages, requests, null, null, null, null, null, test);

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected Task RunTestAsync(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<Stream> shellStreams,
            Func<Task> test) =>
            RunTestAsync(responses, responseMessages, requests, null, null, null, null, shellStreams, test);

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected Task RunTestAsync(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            Func<Task> test) =>
            RunTestAsync(
                responses,
                responseMessages,
                requests,
                syncRequests,
                syncResponses,
                syncDataReceived,
                syncDataSent,
                null,
                test);

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected async Task RunTestAsync(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            IEnumerable<Stream> shellStreams,
            Func<Task> test)
        {
            // If we are running unit tests, we need to mock all the responses
            // that are sent by the device. Do that now.
            if (!IntegrationTest)
            {
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

                if (shellStreams != null)
                {
                    foreach (Stream shellStream in shellStreams)
                    {
                        Socket.ShellStreams.Enqueue(shellStream);
                    }
                }
            }

            Exception exception = null;

            try
            {
                await test();
            }
            catch (AggregateException ex)
            {
                exception = ex.InnerExceptions.Count == 1 ? ex.InnerException : ex;
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
                Assert.Empty(Socket.ShellStreams);

                // Make sure a request was sent
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
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
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                Assert.Equal(responses, Socket.Responses);
                Assert.Equal(responseMessages, Socket.ResponseMessages);

                if (syncResponses != null)
                {
                    Assert.Equal(syncResponses, Socket.SyncResponses);
                }
                else
                {
                    Assert.Empty(Socket.SyncResponses);
                }

                if (syncDataReceived != null)
                {
                    Assert.Equal(syncDataReceived, Socket.SyncDataReceived);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataReceived);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataSent);
                }

                if (shellStreams != null)
                {
                    Assert.Equal(shellStreams, Socket.ShellStreams);
                }
                else
                {
                    Assert.Empty(Socket.ShellStreams);
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        #endregion

        #region Task<TResult>

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which return the result of <paramref name="test"/>.</returns>
        protected Task<TResult> RunTestAsync<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            Func<Task<TResult>> test) =>
            RunTestAsync(responses, responseMessages, requests, null, null, null, null, null, test);

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which return the result of <paramref name="test"/>.</returns>
        protected Task<TResult> RunTestAsync<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<Stream> shellStreams,
            Func<Task<TResult>> test) =>
            RunTestAsync(responses, responseMessages, requests, null, null, null, null, shellStreams, test);

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which return the result of <paramref name="test"/>.</returns>
        protected Task<TResult> RunTestAsync<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            Func<Task<TResult>> test) =>
            RunTestAsync(
                responses,
                responseMessages,
                requests,
                syncRequests,
                syncResponses,
                syncDataReceived,
                syncDataSent,
                null,
                test);

        /// <summary>
        /// <para>
        /// Runs an async ADB helper test, either as a unit test or as an integration test.
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
        /// that the traffic we simulate in the unit tests matches the traffic that is actually sent
        /// over the wire.
        /// </para>
        /// </summary>
        /// <param name="responses">The <see cref="AdbResponse"/> messages that the ADB sever should send.</param>
        /// <param name="responseMessages">The messages that should follow the <paramref name="responses"/>.</param>
        /// <param name="requests">The requests the client should send.</param>
        /// <param name="syncRequests">The <see cref="SyncCommand"/> requests the client should send.</param>
        /// <param name="syncResponses">The <see cref="SyncCommand"/> messages that the ADB sever should send.</param>
        /// <param name="syncDataReceived">The <see cref="Array"/> of <see cref="byte"/> data which the ADB sever should send.</param>
        /// <param name="syncDataSent">The <see cref="Array"/> of <see cref="byte"/> data which the client should send.</param>
        /// <param name="shellStreams">The <see cref="Array"/> of <see cref="Stream"/> which the <see cref="IAdbSocket.GetShellStream"/> should use.</param>
        /// <param name="test">The test to run.</param>
        /// <returns>A <see cref="Task"/> which return the result of <paramref name="test"/>.</returns>
        protected async Task<TResult> RunTestAsync<TResult>(
            IEnumerable<AdbResponse> responses,
            IEnumerable<string> responseMessages,
            IEnumerable<string> requests,
            IEnumerable<(SyncCommand, string)> syncRequests,
            IEnumerable<SyncCommand> syncResponses,
            IEnumerable<byte[]> syncDataReceived,
            IEnumerable<byte[]> syncDataSent,
            IEnumerable<Stream> shellStreams,
            Func<Task<TResult>> test)
        {
            // If we are running unit tests, we need to mock all the responses
            // that are sent by the device. Do that now.
            if (!IntegrationTest)
            {
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

                if (shellStreams != null)
                {
                    foreach (Stream shellStream in shellStreams)
                    {
                        Socket.ShellStreams.Enqueue(shellStream);
                    }
                }
            }

            TResult result = default;
            Exception exception = null;

            try
            {
                result = await test();
            }
            catch (AggregateException ex)
            {
                exception = ex.InnerExceptions.Count == 1 ? ex.InnerException : ex;
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
                Assert.Empty(Socket.ShellStreams);

                // Make sure a request was sent
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
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
                Assert.Equal(requests, Socket.Requests);

                if (syncRequests != null)
                {
                    Assert.Equal(syncRequests, Socket.SyncRequests);
                }
                else
                {
                    Assert.Empty(Socket.SyncRequests);
                }

                Assert.Equal(responses, Socket.Responses);
                Assert.Equal(responseMessages, Socket.ResponseMessages);

                if (syncResponses != null)
                {
                    Assert.Equal(syncResponses, Socket.SyncResponses);
                }
                else
                {
                    Assert.Empty(Socket.SyncResponses);
                }

                if (syncDataReceived != null)
                {
                    Assert.Equal(syncDataReceived, Socket.SyncDataReceived);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataReceived);
                }

                if (syncDataSent != null)
                {
                    Assert.Equal(syncDataSent, Socket.SyncDataSent);
                }
                else
                {
                    Assert.Empty(Socket.SyncDataSent);
                }

                if (shellStreams != null)
                {
                    Assert.Equal(shellStreams, [.. Socket.ShellStreams]);
                }
                else
                {
                    Assert.Empty(Socket.ShellStreams);
                }
            }

            return exception != null ? throw exception : result;
        }

        #endregion

        protected static IEnumerable<AdbResponse> OkResponses(int count) => Enumerable.Repeat(AdbResponse.OK, count);
    }
}
