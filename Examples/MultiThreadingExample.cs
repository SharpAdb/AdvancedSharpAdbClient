using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AdvancedSharpAdbClient;
using System.Threading;
using System.Collections.Concurrent;

// Multi-threading Example for Nox client
namespace MultiThreading
{
    class Program
    {
        static ConcurrentQueue<string> deviceports = new ConcurrentQueue<string>(); // safe

        static object locker = new object();

        static string GetIP()
        {
            while (true)
            {
                Process[] processes = Process.GetProcessesByName("NoxVMHandle");
                foreach (Process process in processes)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c netstat -a -n -o | find \"" + process.Id + "\" | find \"127.0.0.1\" | find \"620\"");
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;

                    var proc = new Process();
                    proc.StartInfo = startInfo;
                    proc.Start();
                    proc.WaitForExit();

                    MatchCollection matches = Regex.Matches(proc.StandardOutput.ReadToEnd(), "(?<=127.0.0.1:)62.*?(?= )");
                    foreach (Match match in matches)
                    {
                        if (match.Value != "" && !deviceports.Contains(match.Value))
                        {
                            deviceports.Enqueue(match.Value);
                            return "127.0.0.1:" + match.Value;
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            GetIP();
            if (!AdbServer.Instance.GetStatus().IsRunning)
            {
                AdbServer server = new AdbServer();
                StartServerResult result = server.StartServer(@"F:\Nox\bin\adb.exe", false);
                if (result != StartServerResult.Started)
                {
                    Console.WriteLine("Can't start adb server");
                    return;
                }
            }
            for (int i = 0; i < 3; i++) // You need to add 3 emulators to multi-drive
            {
                new Thread(() =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName =  @"F:\Nox\bin\Nox.exe";
                    process.StartInfo.Arguments = $"-clone:Nox_{i}"; // Start the i-th emulator
                    process.Start();
                    AdvancedAdbClient client = new AdvancedAdbClient();
                    lock (locker)
                    {
                        client.Connect(GetIP()); // Nox Ip
                    }
                    DeviceData device = client.GetDevices().FirstOrDefault();
                    if (device == null)
                    {
                        Console.WriteLine("Can't connect to device");
                        return;
                    }
                    client.StartApp(device, "com.google.android.youtube");
                    client.Click(device, 1000, 700); // Open first video
                }).Start();
            }
            Console.WriteLine("Started");
            Console.ReadLine();
        }
    }
}
