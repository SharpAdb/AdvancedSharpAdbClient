using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Process tool to run executable for <see cref="AdbCommandLineClient"/>.
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Runs the <paramref name="filename"/> process, invoking a specific <paramref name="command"/>,
        /// and reads the standard output and standard error output.
        /// </summary>
        /// <param name="filename">The process to run.</param>
        /// <param name="command">The command to invoke, such as <c>version</c> or <c>start-server</c>.</param>
        /// <param name="errorOutput">
        /// A list in which to store the standard error output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard
        /// error.
        /// </param>
        /// <param name="standardOutput">
        /// A list in which to store the standard output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard
        /// output.
        /// </param>
        /// <returns>The return code of the process.</returns>
        int RunProcess(string filename, string command, List<string> errorOutput, List<string> standardOutput);
    }

    /// <summary>
    /// The default process tool to run executable for <see cref="AdbCommandLineClient"/>.
    /// </summary>
    public class SystemProcess : IProcess
    {
        /// <inheritdoc/>
        public int RunProcess(string filename, string command, List<string> errorOutput, List<string> standardOutput)
        {
#if !NETSTANDARD1_3
            ProcessStartInfo psi = new ProcessStartInfo(filename, command)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (Process process = Process.Start(psi))
            {
                string standardErrorString = process.StandardError.ReadToEnd();
                string standardOutputString = process.StandardOutput.ReadToEnd();

                errorOutput?.AddRange(standardErrorString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

                standardOutput?.AddRange(standardOutputString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

                // get the return code from the process
                if (!process.WaitForExit(5000))
                {
                    process.Kill();
                }

                return process.ExitCode;
            }
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
