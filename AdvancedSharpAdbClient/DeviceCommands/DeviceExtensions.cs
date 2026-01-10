// <copyright file="DeviceExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Provides extension methods for the <see cref="DeviceData"/> class,
    /// allowing you to run commands directory against a <see cref="DeviceData"/> object.
    /// </summary>
    public static partial class DeviceExtensions
    {
        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="command">The command to execute.</param>
        public static void ExecuteShellCommand(this IAdbClient client, DeviceData device, string command) =>
            client.ExecuteRemoteCommand(command, device);

        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        public static void ExecuteShellCommand(this IAdbClient client, DeviceData device, string command, IShellOutputReceiver? receiver) =>
            client.ExecuteRemoteCommand(command, device, receiver, AdbClient.Encoding);

        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        public static void ExecuteShellCommand(this IAdbClient client, DeviceData device, string command, Func<string, bool>? predicate) =>
            client.ExecuteRemoteCommand(command, device, predicate.AsShellOutputReceiver(), AdbClient.Encoding);

        /// <summary>
        /// Gets the current device screen snapshot.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <returns>A <see cref="XmlDocument"/> containing current hierarchy.</returns>
        public static XmlDocument? DumpScreen(this IAdbClient client, DeviceData device) =>
            new DeviceClient(client, device).DumpScreen();

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="point">The <see cref="Point"/> to click.</param>
        public static void Click(this IAdbClient client, DeviceData device, Point point) =>
            new DeviceClient(client, device).Click(point);

        /// <summary>
        /// Generates a swipe gesture from first coordinates to second coordinates. Specify the speed in ms.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="first">The start <see cref="Point"/>.</param>
        /// <param name="second">The end <see cref="Point"/>.</param>
        /// <param name="speed">The time spent in swiping.</param>
        public static void Swipe(this IAdbClient client, DeviceData device, Point first, Point second, long speed) =>
            new DeviceClient(client, device).Swipe(first, second, speed);

        /// <summary>
        /// Gets the <see cref="AppStatus"/> of the app.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <returns>The <see cref="AppStatus"/> of the app. Foreground, stopped or running in background.</returns>
        public static AppStatus GetAppStatus(this IAdbClient client, DeviceData device, string packageName) =>
            new DeviceClient(client, device).GetAppStatus(packageName);

#if HAS_XPATH
        /// <summary>
        /// Gets element by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="xpath">The xpath of the element.</param>
        /// <param name="timeout">The timeout for waiting the element.
        /// Only check once if <see langword="default"/> or <see cref="TimeSpan.Zero"/>.</param>
        /// <returns>The <see cref="Element"/> of <paramref name="xpath"/>.</returns>
        public static Element? FindElement(this IAdbClient client, DeviceData device, string xpath = "hierarchy/node", TimeSpan timeout = default) =>
            new DeviceClient(client, device).FindElement(xpath, timeout);

        /// <summary>
        /// Gets elements by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="timeout">The timeout for waiting the elements.
        /// Only check once if <see langword="default"/> or <see cref="TimeSpan.Zero"/>.</param>
        /// <returns>The <see cref="IEnumerable{Element}"/> of <see cref="Element"/> has got.</returns>
        public static IEnumerable<Element> FindElements(this IAdbClient client, DeviceData device, string xpath = "hierarchy/node", TimeSpan timeout = default) =>
            new DeviceClient(client, device).FindElements(xpath, timeout);
#endif

        /// <summary>
        /// Send key event to specific. You can see key events here https://developer.android.com/reference/android/view/KeyEvent.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="key">The key event to send.</param>
        public static void SendKeyEvent(this IAdbClient client, DeviceData device, string key) =>
            new DeviceClient(client, device).SendKeyEvent(key);

        /// <summary>
        /// Send text to device. Doesn't support Unicode.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="text">The text to send.</param>
        public static void SendText(this IAdbClient client, DeviceData device, string text) =>
            new DeviceClient(client, device).SendText(text);

        /// <summary>
        /// Clear the input text. The input should be in focus. Use <see cref="Element.ClearInput(int)"/> if the element isn't focused.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="charCount">The length of text to clear.</param>
        public static void ClearInput(this IAdbClient client, DeviceData device, int charCount) =>
            new DeviceClient(client, device).ClearInput(charCount);

        /// <summary>
        /// Click BACK button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click BACK button.</param>
        public static void ClickBackButton(this IAdbClient client, DeviceData device) =>
            new DeviceClient(client, device).ClickBackButton();

        /// <summary>
        /// Click HOME button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        public static void ClickHomeButton(this IAdbClient client, DeviceData device) =>
            new DeviceClient(client, device).ClickHomeButton();

        /// <summary>
        /// Start an Android application on device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        /// <param name="packageName">The package name of the application to start.</param>
        public static void StartApp(this IAdbClient client, DeviceData device, string packageName) =>
            client.ExecuteShellCommand(device, $"monkey -p {packageName} 1");

        /// <summary>
        /// Stop an Android application on device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        /// <param name="packageName">The package name of the application to stop.</param>
        public static void StopApp(this IAdbClient client, DeviceData device, string packageName) =>
            client.ExecuteShellCommand(device, $"am force-stop {packageName}");

        /// <summary>
        /// Pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to pull the file.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.RCV2"/> and <see cref="SyncCommand.STA2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.RECV"/> and <see cref="SyncCommand.STAT"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static void Pull(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream,
            Action<SyncProgressChangedEventArgs>? callback = null,
            bool useV2 = false,
            in bool isCancelled = false)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            service.Pull(remotePath, stream, callback, useV2, in isCancelled);
        }

        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>The <paramref name="permission"/> should coverts to a decimal number. For example, <c>644</c> should be <c>420</c> in decimal, <c>&amp;O644</c> in VB.NET and <c>0o644</c> in F# and Python.</remarks>
        public static void Push(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileStatus permission, DateTimeOffset timestamp,
            Action<SyncProgressChangedEventArgs>? callback = null,
            bool useV2 = false,
            in bool isCancelled = false)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            service.Push(stream, remotePath, permission, timestamp, callback, useV2, in isCancelled);
        }

        /// <summary>
        /// Gets the file statistics of a given file.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to look for the file.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>A <see cref="FileStatistics"/> object that represents the file.</returns>
        public static FileStatistics Stat(this IAdbClient client, DeviceData device, string path)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            return service.Stat(path);
        }

        /// <summary>
        /// Gets the file statistics of a given file (v2).
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to look for the file.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>A <see cref="FileStatisticsEx"/> object that represents the file.</returns>
        /// <remarks>Need Android 8 or above.</remarks>
        public static FileStatisticsEx StatEx(this IAdbClient client, DeviceData device, string path)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            return service.StatEx(path);
        }

        /// <summary>
        /// Gets the file statistics of a given file.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to look for the file.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="Stat(IAdbClient, DeviceData, string)"/>; otherwise, use <see cref="StatEx"/>.</param>
        /// <returns>A <see cref="IFileStatistics"/> object that represents the file.</returns>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static IFileStatistics Stat(this IAdbClient client, DeviceData device, string path, bool useV2) =>
            useV2 ? client.StatEx(device, path) : client.Stat(device, path);

        /// <summary>
        /// Lists the contents of a directory on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to list the directory.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <returns>For each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        public static IEnumerable<FileStatistics> GetDirectoryListing(this IAdbClient client, DeviceData device, string remotePath)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            foreach (FileStatistics fileStatistics in service.GetDirectoryListing(remotePath))
            {
                yield return fileStatistics;
            }
        }

        /// <summary>
        /// Lists the contents of a directory on the device (v2).
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to list the directory.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <returns>For each child item of the directory, a <see cref="FileStatisticsEx"/> object with information of the item.</returns>
        /// <remarks>Need Android 11 or above.</remarks>
        public static IEnumerable<FileStatisticsEx> GetDirectoryListingEx(this IAdbClient client, DeviceData device, string remotePath)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            foreach (FileStatisticsEx fileStatistics in service.GetDirectoryListingEx(remotePath))
            {
                yield return fileStatistics;
            }
        }

        /// <summary>
        /// Lists the contents of a directory on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to list the directory.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="GetDirectoryListing(IAdbClient, DeviceData, string)"/>; otherwise, use <see cref="GetDirectoryListingEx"/>.</param>
        /// <returns>For each child item of the directory, a <see cref="IFileStatistics"/> object with information of the item.</returns>
        /// <remarks>V2 need Android 11 or above.</remarks>
        public static IEnumerable<IFileStatistics> GetDirectoryListing(this IAdbClient client, DeviceData device, string remotePath, bool useV2) =>
#if NETFRAMEWORK && !NET40_OR_GREATER
            useV2 ? client.GetDirectoryListingEx(device, remotePath).OfType<IFileStatistics>() : client.GetDirectoryListing(device, remotePath).OfType<IFileStatistics>();
#else
            useV2 ? client.GetDirectoryListingEx(device, remotePath) : client.GetDirectoryListing(device, remotePath);
#endif

        /// <summary>
        /// Gets the property of a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to get the property.</param>
        /// <param name="property">The name of property which to get.</param>
        /// <returns>The value of the property on the device.</returns>
        public static string GetProperty(this IAdbClient client, DeviceData device, string property)
        {
            ConsoleOutputReceiver receiver = new();
            client.ExecuteShellCommand(device, $"{GetPropReceiver.GetPropCommand} {property}", receiver);
            return receiver.ToString();
        }

        /// <summary>
        /// Gets the properties of a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to list the properties.</param>
        /// <returns>A dictionary containing the properties of the device, and their values.</returns>
        public static Dictionary<string, string> GetProperties(this IAdbClient client, DeviceData device)
        {
            GetPropReceiver receiver = new();
            client.ExecuteShellCommand(device, GetPropReceiver.GetPropCommand, receiver);
            return receiver.Properties;
        }

        /// <summary>
        /// Gets the environment variables currently defined on a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to list the environment variables.</param>
        /// <returns>A dictionary containing the environment variables of the device, and their values.</returns>
        public static Dictionary<string, string> GetEnvironmentVariables(this IAdbClient client, DeviceData device)
        {
            EnvironmentVariablesReceiver receiver = new();
            client.ExecuteShellCommand(device, EnvironmentVariablesReceiver.PrintEnvCommand, receiver);
            return receiver.EnvironmentVariables;
        }

        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public static void InstallPackage(this IAdbClient client, DeviceData device, string packageFilePath, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            manager.InstallPackage(packageFilePath, callback, arguments);
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public static void InstallMultiplePackage(this IAdbClient client, DeviceData device, string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            manager.InstallMultiplePackage(basePackageFilePath, splitPackageFilePaths, callback, arguments);
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public static void InstallMultiplePackage(this IAdbClient client, DeviceData device, IEnumerable<string> splitPackageFilePaths, string packageName, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            manager.InstallMultiplePackage(splitPackageFilePaths, packageName, callback, arguments);
        }

#if !NETFRAMEWORK || NET40_OR_GREATER
        /// <summary>
        /// Pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to pull the file.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.RCV2"/> and <see cref="SyncCommand.STA2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.RECV"/> and <see cref="SyncCommand.STAT"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static void Pull(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream,
            IProgress<SyncProgressChangedEventArgs>? progress = null,
            bool useV2 = false,
            in bool isCancelled = false)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            service.Pull(remotePath, stream, progress.AsAction(), useV2, in isCancelled);
        }

        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>The <paramref name="permission"/> should coverts to a decimal number. For example, <c>644</c> should be <c>420</c> in decimal, <c>&amp;O644</c> in VB.NET and <c>0o644</c> in F# and Python.</remarks>
        public static void Push(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileStatus permission, DateTimeOffset timestamp,
            IProgress<SyncProgressChangedEventArgs>? progress = null,
            bool useV2 = false,
            in bool isCancelled = false)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            service.Push(stream, remotePath, permission, timestamp, progress.AsAction(), useV2, in isCancelled);
        }

        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public static void InstallPackage(this IAdbClient client, DeviceData device, string packageFilePath, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            manager.InstallPackage(packageFilePath, progress, arguments);
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public static void InstallMultiplePackage(this IAdbClient client, DeviceData device, string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            manager.InstallMultiplePackage(basePackageFilePath, splitPackageFilePaths, progress, arguments);
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public static void InstallMultiplePackage(this IAdbClient client, DeviceData device, IEnumerable<string> splitPackageFilePaths, string packageName, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            manager.InstallMultiplePackage(splitPackageFilePaths, packageName, progress, arguments);
        }

#if NET7_0_OR_GREATER
        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 11 or above.</remarks>
        public static void Push(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileMode permission, DateTimeOffset timestamp,
            Action<SyncProgressChangedEventArgs>? callback = null,
            bool useV2 = false,
            in bool isCancelled = false)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            service.Push(stream, remotePath, (UnixFileStatus)permission, timestamp, callback, useV2, in isCancelled);
        }

        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 11 or above.</remarks>
        public static void Push(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileMode permission, DateTimeOffset timestamp,
            IProgress<SyncProgressChangedEventArgs>? progress,
            bool useV2 = false,
            in bool isCancelled = false)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            service.Push(stream, remotePath, (UnixFileStatus)permission, timestamp, progress.AsAction(), useV2, in isCancelled);
        }
#endif
#endif

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="arguments">The arguments to pass to <c>pm uninstall</c>.</param>
        public static void UninstallPackage(this IAdbClient client, DeviceData device, string packageName, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            manager.UninstallPackage(packageName, arguments);
        }

        /// <summary>
        /// Requests the version information from the device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageName">The name of the package from which to get the application version.</param>
        public static VersionInfo GetPackageVersion(this IAdbClient client, DeviceData device, string packageName)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.GetVersionInfo(packageName);
        }

        /// <summary>
        /// Lists all processes running on the device.
        /// </summary>
        /// <param name="client">A connection to ADB.</param>
        /// <param name="device">The device on which to list the processes that are running.</param>
        /// <returns>An <see cref="IEnumerable{AndroidProcess}"/> that will iterate over all processes
        /// that are currently running on the device.</returns>
        public static List<AndroidProcess> ListProcesses(this IAdbClient client, DeviceData device)
        {
            // There are a couple of gotcha's when listing processes on an Android device.
            // One way would be to run ps and parse the output. However, the output of
            // ps different from Android version to Android version, is not delimited, nor
            // entirely fixed length, and some of the fields can be empty, so it's almost impossible
            // to parse correctly.
            //
            // The alternative is to directly read the values in /proc/[pid], pretty much like ps
            // does (see https://android.googlesource.com/platform/system/core/+/master/toolbox/ps.c).
            //
            // The easiest way to do the directory listings would be to use the SyncService; unfortunately,
            // the sync service doesn't work very well with /proc/ so we're back to using ls and taking it
            // from there.

            // List all processes by doing ls /proc/.
            // All subfolders which are completely numeric are PIDs

            // Android 7 and above ships with toybox (https://github.com/landley/toybox), which includes
            // an updated ls which behaves slightly different.
            // The -1 parameter is important to make sure each item gets its own line (it's an assumption we
            // make when parsing output); on Android 7 and above we may see things like:
            // 1     135   160   171 ioports      timer_stats
            // 10    13533 16056 172 irq tty
            // 100   136   16066 173 kallsyms uid_cputime
            // but unfortunately older versions do not handle the -1 parameter well. So we need to branch based
            // on the API level. We do the branching on the device (inside a shell script) to avoid roundtrips.
            // This if/then/else syntax was tested on Android 2.x, 4.x and 7
            ConsoleOutputReceiver receiver = new();
            client.ExecuteShellCommand(
                device,
                "SDK=\"$(/system/bin/getprop ro.build.version.sdk)\"\nif [ $SDK -lt 24 ]; then\n/system/bin/ls /proc/\nelse\n/system/bin/ls -1 /proc/\nfi",
                receiver);

            List<int> pids = [];

            string output = receiver.ToString();
            using (StringReader reader = new(output))
            {
                while (reader.Peek() > 0)
                {
                    string? line = reader.ReadLine();

                    if (line?.All(char.IsDigit) != true)
                    {
                        continue;
                    }

                    int pid = int.Parse(line);

                    pids.Add(pid);
                }
            }

            // For each pid, we can get /proc/[pid]/stat, which contains the process information in a well-defined
            // format - see http://man7.org/linux/man-pages/man5/proc.5.html.
            // Doing cat on each file one by one takes too much time. Doing cat on all of them at the same time doesn't work
            // either, because the command line would be too long.
            // So we do it 25 processes at at time.
            DefaultInterpolatedStringHandler catBuilder = new(3, pids.Count);
            ProcessOutputReceiver processOutputReceiver = new();

            catBuilder.AppendLiteral("cat");

            for (int i = 0; i < pids.Count; i++)
            {
                catBuilder.AppendLiteral(" /proc/");
                catBuilder.AppendFormatted(pids[i]);
                catBuilder.AppendLiteral("/cmdline /proc/");
                catBuilder.AppendFormatted(pids[i]);
                catBuilder.AppendLiteral("/stat");

                if (i > 0 && (i % 25 == 0 || i == pids.Count - 1))
                {
                    client.ExecuteShellCommand(device, catBuilder.ToStringAndClear(), processOutputReceiver);
                    catBuilder.AppendLiteral("cat");
                }
            }

            processOutputReceiver.Flush();

            return processOutputReceiver.Processes;
        }
    }
}
