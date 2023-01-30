using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        public override bool IsValidAdbFile(string adbPath) =>
            // No validation done in the dummy adb client.
            true;

        protected override int RunAdbProcessInner(string command, List<string> errorOutput, List<string> standardOutput)
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

            return 0;
        }

        private static string ServerName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "adb.exe" : "adb";
    }
}
