#if HAS_TASK
// <copyright file="DeviceClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    public partial record class DeviceClient
    {
        /// <summary>
        /// Gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{String}"/> which returns a <see cref="string"/> containing current hierarchy.
        /// Failed if start with <c>ERROR</c> or <c>java.lang.Exception</c>.</returns>
        public async Task<string> DumpScreenStringAsync(CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, "uiautomator dump /dev/tty", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{XmlDocument}"/> which returns a <see cref="XmlDocument"/> containing current hierarchy.</returns>
        public async Task<XmlDocument?> DumpScreenAsync(CancellationToken cancellationToken = default)
        {
            string xmlString = await DumpScreenStringAsync(cancellationToken).ConfigureAwait(false);
            XmlDocument doc = new();
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }

#if HAS_WINRT
        /// <summary>
        /// Gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{XmlDocument}"/> which returns a <see cref="Windows.Data.Xml.Dom.XmlDocument"/> containing current hierarchy.</returns>
#if NET
        [SupportedOSPlatform("Windows10.0.10240.0")]
#endif
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public async Task<Windows.Data.Xml.Dom.XmlDocument?> DumpScreenWinRTAsync(CancellationToken cancellationToken = default)
        {
            string xmlString = await DumpScreenStringAsync(cancellationToken).ConfigureAwait(false);
            Windows.Data.Xml.Dom.XmlDocument doc = new();
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }
#endif

        /// <summary>
        /// Clicks on the specified coordinates asynchronously.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to click.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task ClickAsync(Point point, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"input tap {point.X} {point.Y}", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Clicks on the specified coordinates asynchronously.
        /// </summary>
        /// <param name="x">The X co-ordinate to click.</param>
        /// <param name="y">The Y co-ordinate to click.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task ClickAsync(int x, int y, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"input tap {x} {y}", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Generates a swipe gesture from first element to second element asynchronously. Specify the speed in ms.
        /// </summary>
        /// <param name="first">The start element.</param>
        /// <param name="second">The end element.</param>
        /// <param name="speed">The time spent in swiping.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task SwipeAsync(Element first, Element second, long speed, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"input swipe {first.Center.X} {first.Center.Y} {second.Center.X} {second.Center.Y} {speed}", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Generates a swipe gesture from first coordinates to second coordinates asynchronously. Specify the speed in ms.
        /// </summary>
        /// <param name="first">The start <see cref="Point"/>.</param>
        /// <param name="second">The end <see cref="Point"/>.</param>
        /// <param name="speed">The time spent in swiping.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task SwipeAsync(Point first, Point second, long speed, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"input swipe {first.X} {first.Y} {second.X} {second.Y} {speed}", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Generates a swipe gesture from co-ordinates [x1, y1] to [x2, y2] with speed asynchronously. Specify the speed in ms.
        /// </summary>
        /// <param name="x1">The start X co-ordinate.</param>
        /// <param name="y1">The start Y co-ordinate.</param>
        /// <param name="x2">The end X co-ordinate.</param>
        /// <param name="y2">The end Y co-ordinate.</param>
        /// <param name="speed">The time spent in swiping.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task SwipeAsync(int x1, int y1, int x2, int y2, long speed, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"input swipe {x1} {y1} {x2} {y2} {speed}", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Check if the app is running in foreground asynchronously.
        /// </summary>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Boolean}"/> which returns the result. <see langword="true"/> if the app is running in foreground; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> IsAppRunningAsync(string packageName, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"pidof {packageName}", receiver, cancellationToken).ConfigureAwait(false);

            string? result = receiver.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            bool intParsed = int.TryParse(result, out int pid);
            return intParsed && pid > 0;
        }

        /// <summary>
        /// Check if the app is running in background asynchronously.
        /// </summary>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Boolean}"/> which returns the result. <see langword="true"/> if the app is running in background; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> IsAppInForegroundAsync(string packageName, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { TrimLines = true, ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"dumpsys activity activities | grep mResumedActivity", receiver, cancellationToken).ConfigureAwait(false);

            string result = receiver.ToString();
            return result.Contains(packageName);
        }

        /// <summary>
        /// Gets the <see cref="AppStatus"/> of the app asynchronously.
        /// </summary>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{AppStatus}"/> which returns the <see cref="AppStatus"/> of the app. Foreground, stopped or running in background.</returns>
        public async Task<AppStatus> GetAppStatusAsync(string packageName, CancellationToken cancellationToken = default)
        {
            // Check if the app is in foreground
            bool currentApp = await IsAppInForegroundAsync(packageName, cancellationToken).ConfigureAwait(false);
            if (currentApp)
            {
                return AppStatus.Foreground;
            }

            // Check if the app is running in background
            bool isAppRunning = await IsAppRunningAsync(packageName, cancellationToken).ConfigureAwait(false);
            return isAppRunning ? AppStatus.Background : AppStatus.Stopped;
        }

        /// <summary>
        /// Gets element by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>A <see cref="Task{Element}"/> which returns the <see cref="Element"/> of <paramref name="xpath"/>.</returns>
        public async Task<Element?> FindElementAsync(string xpath = "hierarchy/node", CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        XmlDocument? doc = await DumpScreenAsync(cancellationToken).ConfigureAwait(false);
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
                    if (cancellationToken == default) { break; }
                }
            }
            catch (Exception e)
            {
                // If a cancellation was requested, this main loop is interrupted with an exception
                // because the socket is closed. In that case, we don't need to throw a ShellCommandUnresponsiveException.
                // In all other cases, something went wrong, and we want to report it to the user.
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new ShellCommandUnresponsiveException(e);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets elements by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns the <see cref="List{Element}"/> of <see cref="Element"/> has got.</returns>
        public async Task<IEnumerable<Element>> FindElementsAsync(string xpath = "hierarchy/node", CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        XmlDocument? doc = await DumpScreenAsync(cancellationToken).ConfigureAwait(false);
                        if (doc != null)
                        {
                            XmlNodeList? xmlNodes = doc.SelectNodes(xpath);
                            if (xmlNodes != null)
                            {
                                static IEnumerable<Element> FindElements(IAdbClient client, DeviceData device, XmlNodeList xmlNodes)
                                {
                                    for (int i = 0; i < xmlNodes.Count; i++)
                                    {
                                        Element? element = Element.FromXmlNode(client, device, xmlNodes[i]);
                                        if (element != null)
                                        {
                                            yield return element;
                                        }
                                    }
                                }
                                return FindElements(AdbClient, Device, xmlNodes);
                            }
                        }
                    }
                    catch (XmlException)
                    {
                        // Ignore XmlException and try again
                    }
                    if (cancellationToken == default) { break; }
                }
            }
            catch (Exception e)
            {
                // If a cancellation was requested, this main loop is interrupted with an exception
                // because the socket is closed. In that case, we don't need to throw a ShellCommandUnresponsiveException.
                // In all other cases, something went wrong, and we want to report it to the user.
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new ShellCommandUnresponsiveException(e);
                }
            }
            return [];
        }

#if COMP_NETSTANDARD2_1
        /// <summary>
        /// Gets elements by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>The <see cref="IAsyncEnumerable{Element}"/> of <see cref="Element"/> has got.</returns>
        public async IAsyncEnumerable<Element> FindAsyncElements(string xpath = "hierarchy/node", [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                XmlDocument? doc = null;

                try
                {
                    doc = await DumpScreenAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (XmlException)
                {
                    // Ignore XmlException and try again
                }
                catch (Exception e)
                {
                    // If a cancellation was requested, this main loop is interrupted with an exception
                    // because the socket is closed. In that case, we don't need to throw a ShellCommandUnresponsiveException.
                    // In all other cases, something went wrong, and we want to report it to the user.
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw new ShellCommandUnresponsiveException(e);
                    }
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

                if (cancellationToken == default) { break; }
            }
        }
#endif

        /// <summary>
        /// Send key event to specific asynchronously. You can see key events here https://developer.android.com/reference/android/view/KeyEvent.
        /// </summary>
        /// <param name="key">The key event to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task SendKeyEventAsync(string key, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"input keyevent {key}", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Send text to device asynchronously. Doesn't support Russian.
        /// </summary>
        /// <param name="text">The text to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task SendTextAsync(string text, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new() { ParsesErrors = false };
            await AdbClient.ExecuteShellCommandAsync(Device, $"input text {text}", receiver, cancellationToken).ConfigureAwait(false);

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
        /// Asynchronously clear the input text. The input should be in focus. Use <see cref="Element.ClearInputAsync(int, CancellationToken)"/>  if the element isn't focused.
        /// </summary>
        /// <param name="charCount">The length of text to clear.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task ClearInputAsync(int charCount, CancellationToken cancellationToken = default)
        {
            await SendKeyEventAsync("KEYCODE_MOVE_END", cancellationToken).ConfigureAwait(false);
            await SendKeyEventAsync(StringExtensions.Join(" ", Enumerable.Repeat<string?>("KEYCODE_DEL", charCount)), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously click BACK button.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task ClickBackButtonAsync(CancellationToken cancellationToken = default) => SendKeyEventAsync("KEYCODE_BACK", cancellationToken);

        /// <summary>
        /// Asynchronously click HOME button.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task ClickHomeButtonAsync(CancellationToken cancellationToken = default) => SendKeyEventAsync("KEYCODE_HOME", cancellationToken);

        /// <summary>
        /// Start an Android application on device asynchronously.
        /// </summary>
        /// <param name="packageName">The package name of the application to start.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task StartAppAsync(string packageName, CancellationToken cancellationToken = default) => AdbClient.ExecuteShellCommandAsync(Device, $"monkey -p {packageName} 1", cancellationToken);

        /// <summary>
        /// Stop an Android application on device asynchronously.
        /// </summary>
        /// <param name="packageName">The package name of the application to stop.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task StopAppAsync(string packageName, CancellationToken cancellationToken = default) => AdbClient.ExecuteShellCommandAsync(Device, $"am force-stop {packageName}", cancellationToken);
    }
}
#endif