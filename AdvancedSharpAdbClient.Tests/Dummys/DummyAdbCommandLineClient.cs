using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// 
    /// </summary>
    internal class DummyAdbCommandLineClient : AdbCommandLineClient
    {
        public DummyAdbCommandLineClient() : base(ServerName)
        {
        }

        public Version Version { get; set; }

        public bool ServerStarted { get; private set; }

        // No validation done in the dummy adb client.
        public override bool CheckFileExists(string adbPath) => true;

        protected override int RunProcess(string filename, string command, ICollection<string> errorOutput, ICollection<string> standardOutput)
        {
            if (filename == AdbPath)
            {
                errorOutput?.Add(null);

                standardOutput?.Add(null);

                if (command == "start-server")
                {
                    ServerStarted = true;
                }
                else if (command == "version")
                {
                    if (standardOutput != null && Version != null)
                    {
                        standardOutput.Add($"Android Debug Bridge version {Version.ToString(3)}");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(command));
                }
            }

            return 0;
        }

        protected override Task<int> RunProcessAsync(string filename, string command, ICollection<string> errorOutput, ICollection<string> standardOutput, CancellationToken cancellationToken = default)
        {
            int result = RunProcess(filename, command, errorOutput, standardOutput);
            TaskCompletionSource<int> tcs = new();
            tcs.SetResult(result);
            return tcs.Task;
        }

        private static string ServerName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "adb.exe" : "adb";
    }
}
