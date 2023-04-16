using System.Runtime.InteropServices;
using System;

namespace AdvancedSharpAdbClient.WinRT
{
    internal class WinRTOutputReceiver : AdvancedSharpAdbClient.IShellOutputReceiver
    {
        private readonly IShellOutputReceiver shellOutputReceiver;

        public bool ParsesErrors
        {
            get => shellOutputReceiver.ParsesErrors;
            set => shellOutputReceiver.ParsesErrors = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRTOutputReceiver"/> class.
        /// </summary>
        public WinRTOutputReceiver(IShellOutputReceiver shellOutputReceiver) => this.shellOutputReceiver = shellOutputReceiver;

        public static WinRTOutputReceiver GetShellOutputReceiver(IShellOutputReceiver shellOutputReceiver) => new(shellOutputReceiver);

        public void AddOutput(string line) => shellOutputReceiver?.AddOutput(line);

        public void Flush() => shellOutputReceiver?.Flush();
    }
}
