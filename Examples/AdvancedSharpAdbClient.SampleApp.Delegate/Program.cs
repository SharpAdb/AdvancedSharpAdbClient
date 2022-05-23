using System;
using System.Linq;

namespace AdvancedSharpAdbClient.SampleApp.Delegate
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Any())
            {
                string[] arg = args.FirstOrDefault().Replace("adbsampledelegate:", string.Empty).Split('=');
                switch (arg[0].ToLower())
                {
                    case "startadb":
                        Console.WriteLine("Start ADB");
                        StartADB(arg[1]);
                        break;
                }
            }
            Console.WriteLine("Finnish");
            Console.Write("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static void StartADB(string path)
        {
            Console.WriteLine($"ADB Path: {path}");
            AdbServer server = new AdbServer();
            StartServerResult result = server.StartServer(path, false);
            Console.WriteLine(result.ToString());
        }
    }
}