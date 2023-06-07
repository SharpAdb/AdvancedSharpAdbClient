#if HAS_TASK
// <copyright file="PackageManager.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    public partial class PackageManager
    {
        /// <summary>
        /// Refreshes the packages.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task RefreshPackagesAsync(CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            PackageManagerReceiver pmr = new(Device, this);

            return ThirdPartyOnly
                ? client.ExecuteShellCommandAsync(Device, ListThirdPartyOnly, pmr, cancellationToken)
                : client.ExecuteShellCommandAsync(Device, ListFull, pmr, cancellationToken);
        }

        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="reinstall"><see langword="true"/> if re-install of app should be performed; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallPackageAsync(string packageFilePath, bool reinstall, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            string remoteFilePath = await SyncPackageToDeviceAsync(packageFilePath, OnSyncProgressChanged, cancellationToken);
            await InstallRemotePackageAsync(remoteFilePath, reinstall, cancellationToken);
            await RemoveRemotePackageAsync(remoteFilePath, cancellationToken);

            InstallProgressChanged?.Invoke(this, 100);

            void OnSyncProgressChanged(object sender, SyncProgressChangedEventArgs args)
            {
                InstallProgressChanged?.Invoke(this, args.ProgressPercentage * 0.9);
            }
        }

        /// <summary>
        /// Installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallRemotePackageAsync(string remoteFilePath, bool reinstall, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            string reinstallSwitch = reinstall ? "-r " : string.Empty;

            string cmd = $"pm install {reinstallSwitch}\"{remoteFilePath}\"";
            await client.ExecuteShellCommandAsync(Device, cmd, receiver, cancellationToken);

            InstallProgressChanged?.Invoke(this, 95);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultiplePackageAsync(string basePackageFilePath, IList<string> splitPackageFilePaths, bool reinstall, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            string baseRemoteFilePath = await SyncPackageToDeviceAsync(basePackageFilePath, OnMainSyncProgressChanged, cancellationToken);

            void OnMainSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) => InstallProgressChanged?.Invoke(this, args.ProgressPercentage * 0.45);

            string[] splitRemoteFilePaths = new string[splitPackageFilePaths.Count];
            for (int i = 0; i < splitPackageFilePaths.Count; i++)
            {
                int percent = 45 + (45 * i / splitPackageFilePaths.Count);

                splitRemoteFilePaths[i] = await SyncPackageToDeviceAsync(splitPackageFilePaths[i], OnSplitSyncProgressChanged, cancellationToken);

                void OnSplitSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) => InstallProgressChanged?.Invoke(this, percent + (args.ProgressPercentage * 0.45 / splitPackageFilePaths.Count));
            }

            await InstallMultipleRemotePackageAsync(baseRemoteFilePath, splitRemoteFilePaths, reinstall, cancellationToken);

            for (int i = 0; i < splitRemoteFilePaths.Length; i++)
            {
                string splitRemoteFilePath = splitRemoteFilePaths[i];
                await RemoveRemotePackageAsync(splitRemoteFilePath, cancellationToken);
                InstallProgressChanged?.Invoke(this, 95 + (5 * (i + 1) / (splitRemoteFilePaths.Length + 1)));
            }

            await RemoveRemotePackageAsync(baseRemoteFilePath, cancellationToken);
            InstallProgressChanged?.Invoke(this, 100);
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultiplePackageAsync(IList<string> splitPackageFilePaths, string packageName, bool reinstall, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            string[] splitRemoteFilePaths = new string[splitPackageFilePaths.Count];
            for (int i = 0; i < splitPackageFilePaths.Count; i++)
            {
                int percent = 90 * i / splitPackageFilePaths.Count;

                splitRemoteFilePaths[i] = await SyncPackageToDeviceAsync(splitPackageFilePaths[i], OnSyncProgressChanged, cancellationToken);

                void OnSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) => InstallProgressChanged?.Invoke(this, percent + (args.ProgressPercentage * 0.9 / splitPackageFilePaths.Count));
            }

            await InstallMultipleRemotePackageAsync(splitRemoteFilePaths, packageName, reinstall, cancellationToken);

            for (int i = 0; i < splitRemoteFilePaths.Length; i++)
            {
                string splitRemoteFilePath = splitRemoteFilePaths[i];
                await RemoveRemotePackageAsync(splitRemoteFilePath, cancellationToken);
                InstallProgressChanged?.Invoke(this, 95 + (5 * (i + 1) / splitRemoteFilePaths.Length));
            }
        }

        /// <summary>
        /// Installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="baseRemoteFilePath">The absolute base app file path to package file on device.</param>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultipleRemotePackageAsync(string baseRemoteFilePath, IList<string> splitRemoteFilePaths, bool reinstall, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            string session = await CreateInstallSessionAsync(reinstall, cancellationToken: cancellationToken);

            InstallProgressChanged?.Invoke(this, 91);

            await WriteInstallSessionAsync(session, "base", baseRemoteFilePath, cancellationToken);

            InstallProgressChanged?.Invoke(this, 92);

            int i = 0;
            IEnumerable<Task> tasks = splitRemoteFilePaths.Select(async (splitRemoteFilePath) =>
            {
                try
                {
                    await WriteInstallSessionAsync(session, $"splitapp{i++}", splitRemoteFilePath, cancellationToken);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });

            foreach (Task task in tasks)
            {
                await task;
            }

            InstallProgressChanged?.Invoke(this, 94);

            InstallOutputReceiver receiver = new();
            await client.ExecuteShellCommandAsync(Device, $"pm install-commit {session}", receiver, cancellationToken);

            InstallProgressChanged?.Invoke(this, 95);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultipleRemotePackageAsync(IList<string> splitRemoteFilePaths, string packageName, bool reinstall, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            string session = await CreateInstallSessionAsync(reinstall, packageName, cancellationToken);

            InstallProgressChanged?.Invoke(this, 91);
            
            int i = 0;
            IEnumerable<Task> tasks = splitRemoteFilePaths.Select(async (splitRemoteFilePath) =>
            {
                try
                {
                    await WriteInstallSessionAsync(session, $"splitapp{i++}", splitRemoteFilePath, cancellationToken);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });

            foreach (Task task in tasks)
            {
                await task;
            }

            InstallProgressChanged?.Invoke(this, 93);

            InstallOutputReceiver receiver = new();
            await client.ExecuteShellCommandAsync(Device, $"pm install-commit {session}", receiver, cancellationToken);

            InstallProgressChanged?.Invoke(this, 95);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task UninstallPackageAsync(string packageName, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            await client.ExecuteShellCommandAsync(Device, $"pm uninstall {packageName}", receiver, cancellationToken);
            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Requests the version information from the device.
        /// </summary>
        /// <param name="packageName">The name of the package from which to get the application version.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="VersionInfo"/> of target application.</returns>
        public async Task<VersionInfo> GetVersionInfoAsync(string packageName, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            VersionInfoReceiver receiver = new();
            await client.ExecuteShellCommandAsync(Device, $"dumpsys package {packageName}", receiver, cancellationToken);
            return receiver.VersionInfo;
        }

        /// <summary>
        /// Pushes a file to device
        /// </summary>
        /// <param name="localFilePath">The absolute path to file on local host.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the destination path on device for file.</returns>
        /// <exception cref="IOException">If fatal error occurred when pushing file.</exception>
        private async Task<string> SyncPackageToDeviceAsync(string localFilePath, Action<object, SyncProgressChangedEventArgs> progress, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            try
            {
                string packageFileName = Path.GetFileName(localFilePath);

                // only root has access to /data/local/tmp/... not sure how adb does it then...
                // workitem: 16823
                // workitem: 19711
                string remoteFilePath = LinuxPath.Combine(TempInstallationDirectory, packageFileName);

#if HAS_LOGGER
                logger.LogDebug(packageFileName, $"Uploading {packageFileName} onto device '{Device.Serial}'");
#endif

                using (ISyncService sync = syncServiceFactory(client, Device))
                {
                    if (progress != null)
                    {
                        sync.SyncProgressChanged += (sender, e) => progress(sender, e);
                    }

                    using Stream stream = File.OpenRead(localFilePath);
#if HAS_LOGGER
                    logger.LogDebug($"Uploading file onto device '{Device.Serial}'");
#endif

                    // As C# can't use octal, the octal literal 666 (rw-Permission) is here converted to decimal (438)
                    await sync.PushAsync(stream, remoteFilePath, 438, File.GetLastWriteTime(localFilePath), null, cancellationToken);
                }

                return remoteFilePath;
            }
#if HAS_LOGGER
            catch (IOException e)
            {
                logger.LogError(e, $"Unable to open sync connection! reason: {e.Message}");
#else
            catch (IOException)
            {
#endif
                throw;
            }
        }

        /// <summary>
        /// Remove a file from device.
        /// </summary>
        /// <param name="remoteFilePath">Path on device of file to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <exception cref="IOException">If file removal failed.</exception>
        private async Task RemoveRemotePackageAsync(string remoteFilePath, CancellationToken cancellationToken = default)
        {
            // now we delete the app we synced
            try
            {
                await client.ExecuteShellCommandAsync(Device, $"rm \"{remoteFilePath}\"", null, cancellationToken);
            }
#if HAS_LOGGER
            catch (IOException e)
            {
                logger.LogError(e, $"Failed to delete temporary package: {e.Message}");
#else
            catch (IOException)
            {
#endif
                throw;
            }
        }

        /// <summary>
        /// Like "install", but starts an install session.
        /// </summary>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the session ID.</returns>
        private async Task<string> CreateInstallSessionAsync(bool reinstall, string packageName = null, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            string reinstallSwitch = reinstall ? " -r" : string.Empty;
            string addon = packageName.IsNullOrWhiteSpace() ? string.Empty : $" -p {packageName}";

            string cmd = $"pm install-create{reinstallSwitch}{addon}";
            await client.ExecuteShellCommandAsync(Device, cmd, receiver, cancellationToken);

            if (string.IsNullOrEmpty(receiver.SuccessMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }

            string result = receiver.SuccessMessage;
            int arr = result.IndexOf("]") - 1 - result.IndexOf("[");
            string session = result.Substring(result.IndexOf("[") + 1, arr);

            return session;
        }

        /// <summary>
        /// Write an apk into the given install session.
        /// </summary>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="path">The absolute file path to package file on device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        private async Task WriteInstallSessionAsync(string session, string apkName, string path, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            await client.ExecuteShellCommandAsync(Device, $"pm install-write {session} {apkName}.apk \"{path}\"", receiver, cancellationToken);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }
    }
}
#endif