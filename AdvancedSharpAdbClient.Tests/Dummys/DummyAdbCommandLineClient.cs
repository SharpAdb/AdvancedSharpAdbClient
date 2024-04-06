using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// A mock implementation of the <see cref="IAdbCommandLineClient"/> class.
    /// </summary>
    internal class DummyAdbCommandLineClient() : AdbCommandLineClient(ServerName)
    {
        public AdbCommandLineStatus Version { get; set; }

        public bool ServerStarted { get; private set; }

        // No validation done in the dummy adb client.
        public override bool CheckAdbFileExists(string adbPath) => true;

        public override Task<bool> CheckAdbFileExistsAsync(string adbPath, CancellationToken cancellationToken = default) => Task.FromResult(true);

        protected override int RunProcess(string filename, string command, ICollection<string> errorOutput, ICollection<string> standardOutput, int timeout)
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
                    if (standardOutput != null && Version != default)
                    {
                        standardOutput.AddRange([.. Version]);
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(command));
                }
            }

            return 0;
        }

        protected override async Task<int> RunProcessAsync(string filename, string command, ICollection<string> errorOutput, ICollection<string> standardOutput, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return RunProcess(filename, command, errorOutput, standardOutput, Timeout.Infinite);
        }

        private static string ServerName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "adb.exe" : "adb";
    }
}
