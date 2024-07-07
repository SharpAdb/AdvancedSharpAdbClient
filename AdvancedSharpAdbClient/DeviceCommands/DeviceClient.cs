// <copyright file="DeviceClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// A class which contains methods for interacting with an Android device by <see cref="IAdbClient.ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver, Encoding)"/>
    /// </summary>
    /// <param name="AdbClient">The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.</param>
    /// <param name="Device">The device on which to process command.</param>
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public partial record class DeviceClient(IAdbClient AdbClient, DeviceData Device)
    {
        /// <summary>
        /// The <see cref="IAdbClient"/> to use when communicating with the device.
        /// </summary>
        private IAdbClient adbClient = AdbClient ?? throw new ArgumentNullException(nameof(AdbClient));

        /// <summary>
        /// The device on which to process command.
        /// </summary>
        private DeviceData device = DeviceData.EnsureDevice(ref Device);

        /// <summary>
        /// Gets the <see cref="IAdbClient"/> to use when communicating with the device.
        /// </summary>
        public IAdbClient AdbClient
        {
            get => adbClient;
            init => adbClient = value ?? throw new ArgumentNullException(nameof(AdbClient));
        }

        /// <summary>
        /// Gets the device on which to process command.
        /// </summary>
        public DeviceData Device
        {
            get => device;
            init => device = DeviceData.EnsureDevice(ref value);
        }

        /// <summary>
        /// Gets the current device screen snapshot.
        /// </summary>
        /// <returns>A <see cref="string"/> containing current hierarchy.
        /// Failed if start with <c>ERROR</c> or <c>java.lang.Exception</c>.</returns>
        public string DumpScreenString()
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, "uiautomator dump /dev/tty", receiver);

            string xmlString =
                receiver.ToString()
                        .Replace("Events injected: 1\r\n", string.Empty)
                        .Replace("UI hierchary dumped to: /dev/tty", string.Empty)
                        .Trim();

            if (string.IsNullOrEmpty(xmlString) || xmlString.StartsWith("<?xml"))
            {
                return xmlString;
            }

            Match xmlMatch = GetXmlRegex().Match(xmlString);
            return !xmlMatch.Success ? throw new XmlException("An error occurred while receiving xml: " + xmlString) : xmlMatch.Value;
        }

        /// <summary>
        /// Gets the current device screen snapshot.
        /// </summary>
        /// <returns>A <see cref="XmlDocument"/> containing current hierarchy.</returns>
        public XmlDocument? DumpScreen()
        {
            XmlDocument doc = new();
            string xmlString = DumpScreenString();
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }

#if HAS_WINRT
        /// <summary>
        /// Gets the current device screen snapshot.
        /// </summary>
        /// <returns>A <see cref="Windows.Data.Xml.Dom.XmlDocument"/> containing current hierarchy.</returns>
#if NET
        [SupportedOSPlatform("Windows10.0.10240.0")]
#endif
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public Windows.Data.Xml.Dom.XmlDocument? DumpScreenWinRT()
        {
            Windows.Data.Xml.Dom.XmlDocument doc = new();
            string xmlString = DumpScreenString();
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }
#endif

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to click.</param>
        public void Click(Point point)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"input tap {point.X} {point.Y}", receiver);

            string result = receiver.ToString().Trim();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="x">The X co-ordinate to click.</param>
        /// <param name="y">The Y co-ordinate to click.</param>
        public void Click(int x, int y)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"input tap {x} {y}", receiver);

            string result = receiver.ToString().Trim();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <summary>
        /// Generates a swipe gesture from first element to second element. Specify the speed in ms.
        /// </summary>
        /// <param name="first">The start element.</param>
        /// <param name="second">The end element.</param>
        /// <param name="speed">The time spent in swiping.</param>
        public void Swipe(Element first, Element second, long speed)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"input swipe {first.Center.X} {first.Center.Y} {second.Center.X} {second.Center.Y} {speed}", receiver);

            string result = receiver.ToString().Trim();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <summary>
        /// Generates a swipe gesture from first coordinates to second coordinates. Specify the speed in ms.
        /// </summary>
        /// <param name="first">The start <see cref="Point"/>.</param>
        /// <param name="second">The end <see cref="Point"/>.</param>
        /// <param name="speed">The time spent in swiping.</param>
        public void Swipe(Point first, Point second, long speed)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"input swipe {first.X} {first.Y} {second.X} {second.Y} {speed}", receiver);

            string result = receiver.ToString().Trim();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <summary>
        /// Generates a swipe gesture from co-ordinates [x1, y1] to [x2, y2] with speed. Specify the speed in ms.
        /// </summary>
        /// <param name="x1">The start X co-ordinate.</param>
        /// <param name="y1">The start Y co-ordinate.</param>
        /// <param name="x2">The end X co-ordinate.</param>
        /// <param name="y2">The end Y co-ordinate.</param>
        /// <param name="speed">The time spent in swiping.</param>
        public void Swipe(int x1, int y1, int x2, int y2, long speed)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"input swipe {x1} {y1} {x2} {y2} {speed}", receiver);

            string result = receiver.ToString().Trim();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <summary>
        /// Check if the app is running in foreground.
        /// </summary>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <returns><see langword="true"/> if the app is running in foreground; otherwise, <see langword="false"/>.</returns>
        public bool IsAppRunning(string packageName)
        {
            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"pidof {packageName}", receiver);

            string? result = receiver.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            bool intParsed = int.TryParse(result, out int pid);
            return intParsed && pid > 0;
        }

        /// <summary>
        /// Check if the app is running in background.
        /// </summary>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <returns><see langword="true"/> if the app is running in background; otherwise, <see langword="false"/>.</returns>
        public bool IsAppInForeground(string packageName)
        {
            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"dumpsys activity activities | grep mResumedActivity", receiver);

            string result = receiver.ToString();
            return result.Contains(packageName);
        }

        /// <summary>
        /// Gets the <see cref="AppStatus"/> of the app.
        /// </summary>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <returns>The <see cref="AppStatus"/> of the app. Foreground, stopped or running in background.</returns>
        public AppStatus GetAppStatus(string packageName)
        {
            // Check if the app is in foreground
            bool currentApp = IsAppInForeground(packageName);
            if (currentApp)
            {
                return AppStatus.Foreground;
            }

            // Check if the app is running in background
            bool isAppRunning = IsAppRunning(packageName);
            return isAppRunning ? AppStatus.Background : AppStatus.Stopped;
        }

        /// <summary>
        /// Gets element by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="xpath">The xpath of the element.</param>
        /// <param name="timeout">The timeout for waiting the element.
        /// Only check once if <see langword="default"/> or <see cref="TimeSpan.Zero"/>.</param>
        /// <returns>The <see cref="Element"/> of <paramref name="xpath"/>.</returns>
        public Element? FindElement(string xpath = "hierarchy/node", TimeSpan timeout = default)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            do
            {
                try
                {
                    XmlDocument? doc = DumpScreen();
                    if (doc != null)
                    {
                        XmlNode? xmlNode = doc.SelectSingleNode(xpath);
                        if (xmlNode != null)
                        {
                            Element? element = Element.FromXmlNode(AdbClient, Device, xmlNode);
                            if (element != null)
                            {
                                return element;
                            }
                        }
                    }
                }
                catch (XmlException)
                {
                    // Ignore XmlException and try again
                }
                if (timeout == default) { break; }
            }
            while (stopwatch.Elapsed < timeout);
            return null;
        }

        /// <summary>
        /// Gets elements by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="timeout">The timeout for waiting the elements.
        /// Only check once if <see langword="default"/> or <see cref="TimeSpan.Zero"/>.</param>
        /// <returns>The <see cref="IEnumerable{Element}"/> of <see cref="Element"/> has got.</returns>
        public IEnumerable<Element> FindElements(string xpath = "hierarchy/node", TimeSpan timeout = default)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            do
            {
                XmlDocument? doc = null;

                try
                {
                    doc = DumpScreen();
                }
                catch (XmlException)
                {
                    // Ignore XmlException and try again
                }

                if (doc != null)
                {
                    XmlNodeList? xmlNodes = doc.SelectNodes(xpath);
                    if (xmlNodes != null)
                    {
                        for (int i = 0; i < xmlNodes.Count; i++)
                        {
                            Element? element = Element.FromXmlNode(AdbClient, Device, xmlNodes[i]);
                            if (element != null)
                            {
                                yield return element;
                            }
                        }
                        break;
                    }
                }

                if (timeout == default) { break; }
            }
            while (stopwatch.Elapsed < timeout);
        }

        /// <summary>
        /// Send key event to specific. You can see key events here https://developer.android.com/reference/android/view/KeyEvent.
        /// </summary>
        /// <param name="key">The key event to send.</param>
        public void SendKeyEvent(string key)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"input keyevent {key}", receiver);

            string result = receiver.ToString().Trim();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new InvalidKeyEventException("KeyEvent is invalid");
            }
        }

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="text">The text to send.</param>
        public void SendText(string text)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            AdbClient.ExecuteShellCommand(Device, $"input text {text}", receiver);

            string result = receiver.ToString().Trim();

            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new InvalidTextException();
            }
        }

        /// <summary>
        /// Clear the input text. The input should be in focus. Use <see cref="Element.ClearInput(int)"/> if the element isn't focused.
        /// </summary>
        /// <param name="charCount">The length of text to clear.</param>
        public void ClearInput(int charCount)
        {
            SendKeyEvent("KEYCODE_MOVE_END");
            SendKeyEvent(StringExtensions.Join(" ", Enumerable.Repeat<string?>("KEYCODE_DEL", charCount)));
        }

        /// <summary>
        /// Click BACK button.
        /// </summary>
        public void ClickBackButton() => SendKeyEvent("KEYCODE_BACK");

        /// <summary>
        /// Click HOME button.
        /// </summary>
        public void ClickHomeButton() => SendKeyEvent("KEYCODE_HOME");

        /// <summary>
        /// Start an Android application on device.
        /// </summary>
        /// <param name="packageName">The package name of the application to start.</param>
        public void StartApp(string packageName) => AdbClient.ExecuteShellCommand(Device, $"monkey -p {packageName} 1");

        /// <summary>
        /// Stop an Android application on device.
        /// </summary>
        /// <param name="packageName">The package name of the application to stop.</param>
        public void StopApp(string packageName) => AdbClient.ExecuteShellCommand(Device, $"am force-stop {packageName}");

        /// <summary>
        /// Deconstruct the <see cref="DeviceClient"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.</param>
        /// <param name="device">The device on which to process command.</param>
        public void Deconstruct(out IAdbClient client, out DeviceData device)
        {
            client = AdbClient;
            device = Device;
        }

#if !NET40_OR_GREATER && !NETCOREAPP2_0_OR_GREATER && !NETSTANDARD2_0_OR_GREATER && !UAP10_0_15138_0
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(EqualityContract, AdbClient, Device);

        /// <inheritdoc/>
        public virtual bool Equals(DeviceClient? other) =>
            (object?)this == other ||
                (other != (object?)null
                && EqualityComparer<Type>.Default.Equals(EqualityContract, other.EqualityContract)
                && EqualityComparer<IAdbClient>.Default.Equals(AdbClient, other.AdbClient)
                && EqualityComparer<DeviceData>.Default.Equals(Device, other.Device));
#endif

#if NET7_0_OR_GREATER
        [GeneratedRegex(@"<\?xml(.?)*")]
        private static partial Regex GetXmlRegex();
#else
        /// <summary>
        /// Gets a <see cref="Regex"/> for parsing the xml.
        /// </summary>
        /// <returns>The <see cref="Regex"/> for parsing the xml.</returns>
        private static Regex GetXmlRegex() => new(@"<\?xml(.?)*");
#endif
    }
}
