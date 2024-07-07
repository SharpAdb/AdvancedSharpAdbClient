#if HAS_TASK
// <copyright file="DeviceExtensions.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    public static partial class DeviceExtensions
    {
        /// <summary>
        /// Asynchronously executes a shell command on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteShellCommandAsync(this IAdbClient client, DeviceData device, string command, CancellationToken cancellationToken = default) =>
            client.ExecuteRemoteCommandAsync(command, device, cancellationToken);

        /// <summary>
        /// Asynchronously executes a shell command on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteShellCommandAsync(this IAdbClient client, DeviceData device, string command, IShellOutputReceiver? receiver, CancellationToken cancellationToken = default) =>
            client.ExecuteRemoteCommandAsync(command, device, receiver, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Asynchronously executes a shell command on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteShellCommandAsync(this IAdbClient client, DeviceData device, string command, Func<string, bool>? predicate, CancellationToken cancellationToken = default) =>
            client.ExecuteRemoteCommandAsync(command, device, predicate.AsShellOutputReceiver(), AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Asynchronously gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{XmlDocument}"/> which returns a <see cref="XmlDocument"/> containing current hierarchy.</returns>
        public static Task<XmlDocument?> DumpScreenAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).DumpScreenAsync(cancellationToken);

        /// <summary>
        /// Asynchronously clicks on the specified coordinates.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="point">The <see cref="Point"/> to click.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ClickAsync(this IAdbClient client, DeviceData device, Point point, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).ClickAsync(point, cancellationToken);

        /// <summary>
        /// Asynchronously generates a swipe gesture from first coordinates to second coordinates. Specify the speed in ms.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="first">The start <see cref="Point"/>.</param>
        /// <param name="second">The end <see cref="Point"/>.</param>
        /// <param name="speed">The time spent in swiping.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task SwipeAsync(this IAdbClient client, DeviceData device, Point first, Point second, long speed, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).SwipeAsync(first, second, speed, cancellationToken);

        /// <summary>
        /// Asynchronously get the <see cref="AppStatus"/> of the app.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{AppStatus}"/> which returns the <see cref="AppStatus"/> of the app. Foreground, stopped or running in background.</returns>
        public static Task<AppStatus> GetAppStatusAsync(this IAdbClient client, DeviceData device, string packageName, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).GetAppStatusAsync(packageName, cancellationToken);

        /// <summary>
        /// Asynchronously get element by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>A <see cref="Task{Element}"/> which returns the <see cref="Element"/> of <paramref name="xpath"/>.</returns>
        public static Task<Element?> FindElementAsync(this IAdbClient client, DeviceData device, string xpath = "hierarchy/node", CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).FindElementAsync(xpath, cancellationToken);

        /// <summary>
        /// Asynchronously get elements by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns the <see cref="List{Element}"/> of <see cref="Element"/> has got.</returns>
        public static Task<IEnumerable<Element>> FindElementsAsync(this IAdbClient client, DeviceData device, string xpath = "hierarchy/node", CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).FindElementsAsync(xpath, cancellationToken);

        /// <summary>
        /// Asynchronously send key event to specific. You can see key events here https://developer.android.com/reference/android/view/KeyEvent.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="key">The key event to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task SendKeyEventAsync(this IAdbClient client, DeviceData device, string key, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).SendKeyEventAsync(key, cancellationToken);

        /// <summary>
        /// Asynchronously send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="text">The text to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task SendTextAsync(this IAdbClient client, DeviceData device, string text, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).SendTextAsync(text, cancellationToken);

        /// <summary>
        /// Asynchronously clear the input text. The input should be in focus. Use <see cref="Element.ClearInputAsync(int, CancellationToken)"/>  if the element isn't focused.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="charCount">The length of text to clear.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ClearInputAsync(this IAdbClient client, DeviceData device, int charCount, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).ClearInputAsync(charCount, cancellationToken);

        /// <summary>
        /// Asynchronously click BACK button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click BACK button.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ClickBackButtonAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).ClickBackButtonAsync(cancellationToken);

        /// <summary>
        /// Asynchronously click HOME button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ClickHomeButtonAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default) =>
            new DeviceClient(client, device).ClickHomeButtonAsync(cancellationToken);

        /// <summary>
        /// Asynchronously start an Android application on device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        /// <param name="packageName">The package name of the application to start.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task StartAppAsync(this IAdbClient client, DeviceData device, string packageName, CancellationToken cancellationToken = default) =>
            client.ExecuteShellCommandAsync(device, $"monkey -p {packageName} 1", cancellationToken);

        /// <summary>
        /// Asynchronously stop an Android application on device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        /// <param name="packageName">The package name of the application to stop.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task StopAppAsync(this IAdbClient client, DeviceData device, string packageName, CancellationToken cancellationToken = default) =>
            client.ExecuteShellCommandAsync(device, $"am force-stop {packageName}", cancellationToken);

        /// <summary>
        /// Asynchronously pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to pull the file.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PullAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream,
            Action<SyncProgressChangedEventArgs>? callback = null,
            CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            await service.PullAsync(remotePath, stream, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PushAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileStatus permission, DateTimeOffset timestamp,
            Action<SyncProgressChangedEventArgs>? callback = null,
            CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            await service.PushAsync(stream, remotePath, permission, timestamp, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets the file statistics of a given file.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to look for the file.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{FileStatistics}"/> which returns a <see cref="FileStatistics"/> object that contains information about the file.</returns>
        public static async Task<FileStatistics> StatAsync(this IAdbClient client, DeviceData device, string path, CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            return await service.StatAsync(path, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously lists the contents of a directory on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to list the directory.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{List}"/> which returns for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        public static async Task<List<FileStatistics>> GetDirectoryListingAsync(this IAdbClient client, DeviceData device, string remotePath, CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            return await service.GetDirectoryListingAsync(remotePath, cancellationToken).ConfigureAwait(false);
        }

#if COMP_NETSTANDARD2_1
        /// <summary>
        /// Asynchronously lists the contents of a directory on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to list the directory.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>An <see cref="IAsyncEnumerable{FileStatistics}"/> which returns for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        public static async IAsyncEnumerable<FileStatistics> GetDirectoryAsyncListing(this IAdbClient client, DeviceData device, string remotePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            await foreach (FileStatistics file in service.GetDirectoryAsyncListing(remotePath, cancellationToken).ConfigureAwait(false))
            {
                yield return file;
            }
        }
#endif

        /// <summary>
        /// Asynchronously gets the property of a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to get the property.</param>
        /// <param name="property">The name of property which to get.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the value of the property on the device.</returns>
        public static async Task<string> GetPropertyAsync(this IAdbClient client, DeviceData device, string property, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new();
            await client.ExecuteShellCommandAsync(device, $"{GetPropReceiver.GetPropCommand} {property}", receiver, cancellationToken).ConfigureAwait(false);
            return receiver.ToString();
        }

        /// <summary>
        /// Asynchronously gets the properties of a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to list the properties.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{Dictionary}"/> which returns a dictionary containing the properties of the device, and their values.</returns>
        public static async Task<Dictionary<string, string>> GetPropertiesAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default)
        {
            GetPropReceiver receiver = new();
            await client.ExecuteShellCommandAsync(device, GetPropReceiver.GetPropCommand, receiver, cancellationToken).ConfigureAwait(false);
            return receiver.Properties;
        }

        /// <summary>
        /// Asynchronously gets the environment variables currently defined on a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to list the environment variables.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{Dictionary}"/> which returns the a dictionary containing the environment variables of the device, and their values.</returns>
        public static async Task<Dictionary<string, string>> GetEnvironmentVariablesAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnvironmentVariablesReceiver receiver = new();
            await client.ExecuteShellCommandAsync(device, EnvironmentVariablesReceiver.PrintEnvCommand, receiver, cancellationToken).ConfigureAwait(false);
            return receiver.EnvironmentVariables;
        }

        /// <summary>
        /// Asynchronously installs an Android application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallPackageAsync(this IAdbClient client, DeviceData device, string packageFilePath, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.InstallPackageAsync(packageFilePath, callback, cancellationToken, arguments);
        }

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallMultiplePackageAsync(this IAdbClient client, DeviceData device, string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.InstallMultiplePackageAsync(basePackageFilePath, splitPackageFilePaths, callback, cancellationToken, arguments);
        }

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallMultiplePackageAsync(this IAdbClient client, DeviceData device, IEnumerable<string> splitPackageFilePaths, string packageName, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.InstallMultiplePackageAsync(splitPackageFilePaths, packageName, callback, cancellationToken, arguments);
        }

#if !NETFRAMEWORK || NET40_OR_GREATER
        /// <summary>
        /// Asynchronously pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to pull the file.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PullAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream,
            IProgress<SyncProgressChangedEventArgs>? progress,
            CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            await service.PullAsync(remotePath, stream, progress.AsAction(), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PushAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileStatus permission, DateTimeOffset timestamp,
            IProgress<SyncProgressChangedEventArgs>? progress,
            CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            await service.PushAsync(stream, remotePath, permission, timestamp, progress.AsAction(), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously installs an Android application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallPackageAsync(this IAdbClient client, DeviceData device, string packageFilePath, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.InstallPackageAsync(packageFilePath, progress, cancellationToken, arguments);
        }

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallMultiplePackageAsync(this IAdbClient client, DeviceData device, string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.InstallMultiplePackageAsync(basePackageFilePath, splitPackageFilePaths, progress, cancellationToken, arguments);
        }

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallMultiplePackageAsync(this IAdbClient client, DeviceData device, IEnumerable<string> splitPackageFilePaths, string packageName, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.InstallMultiplePackageAsync(splitPackageFilePaths, packageName, progress, cancellationToken, arguments);
        }

#if NET7_0_OR_GREATER
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PushAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileMode permission, DateTimeOffset timestamp,
            Action<SyncProgressChangedEventArgs>? callback = null,
            CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            await service.PushAsync(stream, remotePath, (UnixFileStatus)permission, timestamp, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PushAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, UnixFileMode permission, DateTimeOffset timestamp,
            IProgress<SyncProgressChangedEventArgs>? progress,
            CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            await service.PushAsync(stream, remotePath, (UnixFileStatus)permission, timestamp, progress.AsAction(), cancellationToken).ConfigureAwait(false);
        }
#endif
#endif

        /// <summary>
        /// Asynchronously uninstalls a package from the device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="arguments">The arguments to pass to <c>pm uninstall</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task UninstallPackageAsync(this IAdbClient client, DeviceData device, string packageName, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.UninstallPackageAsync(packageName, arguments);
        }

        /// <summary>
        /// Asynchronously uninstalls a package from the device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm uninstall</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task UninstallPackageAsync(this IAdbClient client, DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.UninstallPackageAsync(packageName, cancellationToken, arguments);
        }

        /// <summary>
        /// Asynchronously requests the version information from the device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageName">The name of the package from which to get the application version.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{VersionInfo}"/> which returns the <see cref="VersionInfo"/> of target application.</returns>
        public static Task<VersionInfo> GetPackageVersionAsync(this IAdbClient client, DeviceData device, string packageName, CancellationToken cancellationToken = default)
        {
            PackageManager manager = new(client, device, skipInit: true);
            return manager.GetVersionInfoAsync(packageName, cancellationToken);
        }

        /// <summary>
        /// Asynchronously lists all processes running on the device.
        /// </summary>
        /// <param name="client">A connection to ADB.</param>
        /// <param name="device">The device on which to list the processes that are running.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns the an <see cref="IEnumerable{AndroidProcess}"/> that will iterate over all processes
        /// that are currently running on the device.</returns>
        public static async Task<List<AndroidProcess>> ListProcessesAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default)
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
            await client.ExecuteShellCommandAsync(
                device,
                "SDK=\"$(/system/bin/getprop ro.build.version.sdk)\"\nif [ $SDK -lt 24 ]; then\n/system/bin/ls /proc/\nelse\n/system/bin/ls -1 /proc/\nfi",
                receiver,
                cancellationToken).ConfigureAwait(false);

            List<int> pids = [];

            string output = receiver.ToString();
            using (StringReader reader = new(output))
            {
                while (reader.Peek() > 0)
                {
                    string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

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
            StringBuilder catBuilder = new();
            ProcessOutputReceiver processOutputReceiver = new();

            _ = catBuilder.Append("cat");

            for (int i = 0; i < pids.Count; i++)
            {
                _ = catBuilder.Append(" /proc/").Append(pids[i]).Append("/cmdline /proc/").Append(pids[i]).Append("/stat");

                if (i > 0 && (i % 25 == 0 || i == pids.Count - 1))
                {
                    await client.ExecuteShellCommandAsync(device, catBuilder.ToString(), processOutputReceiver, cancellationToken).ConfigureAwait(false);
                    _ = catBuilder.Clear().Append("cat");
                }
            }

            processOutputReceiver.Flush();

            return processOutputReceiver.Processes;
        }
    }
}
#endif