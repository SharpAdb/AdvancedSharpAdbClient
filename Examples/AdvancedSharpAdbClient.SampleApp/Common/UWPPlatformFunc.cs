using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.SampleApp.Common
{
    internal static class UWPPlatformFunc
    {
        public static bool CheckFileExists(string path)
        {
            return !string.IsNullOrWhiteSpace(path);
        }

        public static int RunProcess(string filename, string command, List<string> errorOutput, List<string> standardOutput)
        {
            ProcessStartInfo psi = new(filename, command)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using Process process = Process.Start(psi);
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

        public static TResult AwaitByTaskCompleteSource<TResult>(Func<Task<TResult>> func)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new();
            Task<TResult> task1 = taskCompletionSource.Task;
            _ = Task.Run(async () =>
            {
                TResult result = await func.Invoke();
                taskCompletionSource.SetResult(result);
            });
            TResult task1Result = task1.Result;
            return task1Result;
        }
    }
}
