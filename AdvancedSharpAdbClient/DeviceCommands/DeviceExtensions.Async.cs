#if HAS_TASK
// <copyright file="DeviceExtensions.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    public static partial class DeviceExtensions
    {
        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteShellCommandAsync(this IAdbClient client, DeviceData device, string command, IShellOutputReceiver receiver, CancellationToken cancellationToken = default) =>
            client.ExecuteRemoteCommandAsync(command, device, receiver, cancellationToken);

        /// <summary>
        /// Gets the file statistics of a given file.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to look for the file.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="FileStatistics"/> object that contains information about the file.</returns>
        public static async Task<FileStatistics> StatAsync(this IAdbClient client, DeviceData device, string path, CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            return await service.StatAsync(path, cancellationToken);
        }

        /// <summary>
        /// Lists the contents of a directory on the device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to list the directory.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        public static async Task<IEnumerable<FileStatistics>> List(this IAdbClient client, DeviceData device, string remotePath, CancellationToken cancellationToken = default)
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            return await service.GetDirectoryListingAsync(remotePath, cancellationToken);
        }

        /// <summary>
        /// Pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to pull the file.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="syncProgressEventHandler">An optional handler for the <see cref="ISyncService.SyncProgressChanged"/> event.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PullAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream,
            EventHandler<SyncProgressChangedEventArgs> syncProgressEventHandler = null,
            IProgress<int> progress = null, CancellationToken cancellationToken = default            )
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            if (syncProgressEventHandler != null)
            {
                service.SyncProgressChanged += syncProgressEventHandler;
            }

            await service.PullAsync(remotePath, stream, progress, cancellationToken);
        }

        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use when executing the command.</param>
        /// <param name="device">The device on which to put the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="permissions">The permission octet that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="syncProgressEventHandler">An optional handler for the <see cref="ISyncService.SyncProgressChanged"/> event.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task PushAsync(this IAdbClient client, DeviceData device,
            string remotePath, Stream stream, int permissions, DateTimeOffset timestamp,
            EventHandler<SyncProgressChangedEventArgs> syncProgressEventHandler = null,
            IProgress<int> progress = null, CancellationToken cancellationToken = default            )
        {
            using ISyncService service = Factories.SyncServiceFactory(client, device);
            if (syncProgressEventHandler != null)
            {
                service.SyncProgressChanged += syncProgressEventHandler;
            }

            await service.PushAsync(stream, remotePath, permissions, timestamp, progress, cancellationToken);
        }

        /// <summary>
        /// Gets the property of a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to get the property.</param>
        /// <param name="property">The name of property which to get.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return the value of the property on the device.</returns>
        public static async Task<string> GetPropertyAsync(this IAdbClient client, DeviceData device, string property, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new();
            await client.ExecuteRemoteCommandAsync($"{GetPropReceiver.GetPropCommand} {property}", device, receiver, cancellationToken);
            return receiver.ToString();
        }

        /// <summary>
        /// Gets the properties of a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to list the properties.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return a dictionary containing the properties of the device, and their values.</returns>
        public static async Task<Dictionary<string, string>> GetPropertiesAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default)
        {
            GetPropReceiver receiver = new();
            await client.ExecuteRemoteCommandAsync(GetPropReceiver.GetPropCommand, device, receiver, cancellationToken);
            return receiver.Properties;
        }

        /// <summary>
        /// Gets the environment variables currently defined on a device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device for which to list the environment variables.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return the a dictionary containing the environment variables of the device, and their values.</returns>
        public static async Task<Dictionary<string, string>> GetEnvironmentVariablesAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnvironmentVariablesReceiver receiver = new();
            await client.ExecuteRemoteCommandAsync(EnvironmentVariablesReceiver.PrintEnvCommand, device, receiver, cancellationToken);
            return receiver.EnvironmentVariables;
        }

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task UninstallPackageAsync(this IAdbClient client, DeviceData device, string packageName, CancellationToken cancellationToken = default)
        {
            PackageManager manager = new(client, device);
            return manager.UninstallPackageAsync(packageName, cancellationToken);
        }

        /// <summary>
        /// Requests the version information from the device.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to uninstall the package.</param>
        /// <param name="packageName">The name of the package from which to get the application version.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="VersionInfo"/> of target application.</returns>
        public static Task<VersionInfo> GetPackageVersionAsync(this IAdbClient client, DeviceData device, string packageName, CancellationToken cancellationToken = default)
        {
            PackageManager manager = new(client, device);
            return manager.GetVersionInfoAsync(packageName, cancellationToken);
        }

        /// <summary>
        /// Lists all processes running on the device.
        /// </summary>
        /// <param name="client">A connection to ADB.</param>
        /// <param name="device">The device on which to list the processes that are running.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return the an <see cref="IEnumerable{AndroidProcess}"/> that will iterate over all processes
        /// that are currently running on the device.</returns>
        public static async Task<IEnumerable<AndroidProcess>> ListProcessesAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default)
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
            List<AndroidProcess> processes = new();

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
            await client.ExecuteShellCommandAsync(device, @"SDK=""$(/system/bin/getprop ro.build.version.sdk)""
if [ $SDK -lt 24 ]
then
    /system/bin/ls /proc/
else
    /system/bin/ls -1 /proc/
fi".Replace("\r\n", "\n"), receiver, cancellationToken);

            Collection<int> pids = new();

            string output = receiver.ToString();
            using (StringReader reader = new(output))
            {
                while (reader.Peek() > 0)
                {
                    string line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                    if (!line.All(char.IsDigit))
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

            _ = catBuilder.Append("cat ");

            for (int i = 0; i < pids.Count; i++)
            {
                _ = catBuilder.Append($"/proc/{pids[i]}/cmdline /proc/{pids[i]}/stat ");

                if (i > 0 && (i % 25 == 0 || i == pids.Count - 1))
                {
                    await client.ExecuteShellCommandAsync(device, catBuilder.ToString(), processOutputReceiver, cancellationToken);
                    _ = catBuilder.Clear();
                    _ = catBuilder.Append("cat ");
                }
            }

            processOutputReceiver.Flush();

            return processOutputReceiver.Processes;
        }
    }
}
#endif