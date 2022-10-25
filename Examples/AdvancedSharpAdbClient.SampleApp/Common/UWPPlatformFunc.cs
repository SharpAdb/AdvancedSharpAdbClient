using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DataReceivedEventArgs = ProcessForUWP.UWP.DataReceivedEventArgs;
using Process = ProcessForUWP.UWP.Process;

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
            int code = 1;

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
                CancellationTokenSource Token = new CancellationTokenSource();

                process.BeginOutputReadLine();

                process.EnableRaisingEvents = true;

                void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data == null)
                    {
                        code = 0;
                        Token.Cancel();
                        return;
                    }
                    string line = e.Data ?? string.Empty;

                    standardOutput?.Add(line);
                }

                void ErrorDataReceived(object sender, DataReceivedEventArgs e)
                {
                    string line = e.Data ?? string.Empty;

                    errorOutput?.Add(line);
                }

                try
                {
                    process.OutputDataReceived += OnOutputDataReceived;
                    process.ErrorDataReceived += ErrorDataReceived;
                    while (!process.IsExited)
                    {
                        Token.Token.ThrowIfCancellationRequested();
                    }
                }
                catch (Exception)
                {
                    process.Kill();
                }
                finally
                {
                    process.Close();
                    process.OutputDataReceived -= OnOutputDataReceived;
                }
            }

            return code;
        }

        public static TResult AwaitByTaskCompleteSource<TResult>(Func<Task<TResult>> func)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
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
