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

            InstallProgressChanged?.Invoke(this, new InstallProgress(0, 1, PackageInstallProgressState.PostInstall));
            await RemoveRemotePackageAsync(remoteFilePath, cancellationToken);
            InstallProgressChanged?.Invoke(this, new InstallProgress(1, 1, PackageInstallProgressState.PostInstall));

            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.Finished));

            void OnSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) =>
                InstallProgressChanged?.Invoke(this, new InstallProgress(sender is true ? 1 : 0, 1, args.ProgressPercentage));
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
            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.Installing));

            ValidateDevice();

            InstallOutputReceiver receiver = new();
            string reinstallSwitch = reinstall ? "-r " : string.Empty;

            string cmd = $"pm install {reinstallSwitch}\"{remoteFilePath}\"";
            await client.ExecuteShellCommandAsync(Device, cmd, receiver, cancellationToken);

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

            void OnMainSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) =>
                InstallProgressChanged?.Invoke(this, new InstallProgress(sender is true ? 1 : 0, splitPackageFilePaths.Count + 1, args.ProgressPercentage / 2));

            string baseRemoteFilePath = await SyncPackageToDeviceAsync(basePackageFilePath, OnMainSyncProgressChanged, cancellationToken);

            Dictionary<string, double> progress = new();
            void OnSplitSyncProgressChanged(object sender, SyncProgressChangedEventArgs args)
            {
                int count = 1;
                if (sender is string path)
                {
                    if (!progress.ContainsKey(path))
                    {
                        progress.Add(path, args.ProgressPercentage);
                    }
                    else
                    {
                        progress[path] = args.ProgressPercentage;
                    }
                }
                else if (sender is true)
                {
                    count++;
                }

                double present = 0;
                foreach (KeyValuePair<string, double> info in progress)
                {
                    present += (info.Value / splitPackageFilePaths.Count) / 2;
                }

                InstallProgressChanged?.Invoke(this, new InstallProgress(count, splitPackageFilePaths.Count + 1, present));
            }

            List<string> splitRemoteFilePaths = new();
            IEnumerable<Task> tasks = splitPackageFilePaths.Select(async (x) => splitRemoteFilePaths.Add(await SyncPackageToDeviceAsync(x, OnSplitSyncProgressChanged, cancellationToken)));
            foreach (Task task in tasks)
            {
                await task;
            }

            await InstallMultipleRemotePackageAsync(baseRemoteFilePath, splitRemoteFilePaths, reinstall, cancellationToken);

            InstallProgressChanged?.Invoke(this, new InstallProgress(0, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.PostInstall));
            int count = 0;
            tasks = splitRemoteFilePaths.Select(async (x) =>
            {
                await RemoveRemotePackageAsync(x, cancellationToken);
                InstallProgressChanged?.Invoke(this, new InstallProgress(++count, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.PostInstall));
            });
            foreach (Task task in tasks)
            {
                await task;
            }

            await RemoveRemotePackageAsync(baseRemoteFilePath, cancellationToken);
            InstallProgressChanged?.Invoke(this, new InstallProgress(++count, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.PostInstall));

            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.Finished));
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

            Dictionary<string, double> progress = new();
            void OnSyncProgressChanged(object sender, SyncProgressChangedEventArgs args)
            {
                int count = 1;
                if (sender is string path)
                {
                    if (!progress.ContainsKey(path))
                    {
                        progress.Add(path, args.ProgressPercentage);
                    }
                    else
                    {
                        progress[path] = args.ProgressPercentage;
                    }
                }
                else if (sender is true)
                {
                    count++;
                }

                double present = 0;
                foreach (KeyValuePair<string, double> info in progress)
                {
                    present += (info.Value / splitPackageFilePaths.Count) / 2;
                }

                InstallProgressChanged?.Invoke(this, new InstallProgress(count, splitPackageFilePaths.Count, present));
            }

            List<string> splitRemoteFilePaths = new();
            IEnumerable<Task> tasks = splitPackageFilePaths.Select(async (x) => splitRemoteFilePaths.Add(await SyncPackageToDeviceAsync(x, OnSyncProgressChanged, cancellationToken)));
            foreach (Task task in tasks)
            {
                await task;
            }

            await InstallMultipleRemotePackageAsync(splitRemoteFilePaths, packageName, reinstall, cancellationToken);

            InstallProgressChanged?.Invoke(this, new InstallProgress(0, splitRemoteFilePaths.Count, PackageInstallProgressState.PostInstall));
            int count = 0;
            tasks = splitRemoteFilePaths.Select(async (x) =>
            {
                await RemoveRemotePackageAsync(x, cancellationToken);
                InstallProgressChanged?.Invoke(this, new InstallProgress(++count, splitRemoteFilePaths.Count, PackageInstallProgressState.PostInstall));
            });
            foreach (Task task in tasks)
            {
                await task;
            }

            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.Finished));
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
            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = await CreateInstallSessionAsync(reinstall, cancellationToken: cancellationToken);

            InstallProgressChanged?.Invoke(this, new InstallProgress(0, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));

            await WriteInstallSessionAsync(session, "base", baseRemoteFilePath, cancellationToken);

            InstallProgressChanged?.Invoke(this, new InstallProgress(1, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));

            int i = 0, count = 0;
            IEnumerable<Task> tasks = splitRemoteFilePaths.Select(async (splitRemoteFilePath) =>
            {
                try
                {
                    await WriteInstallSessionAsync(session, $"splitapp{i++}", splitRemoteFilePath, cancellationToken);
                    InstallProgressChanged?.Invoke(this, new InstallProgress(++count, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));
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

            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            await client.ExecuteShellCommandAsync(Device, $"pm install-commit {session}", receiver, cancellationToken);

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
            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = await CreateInstallSessionAsync(reinstall, packageName, cancellationToken);

            InstallProgressChanged?.Invoke(this, new InstallProgress(0, splitRemoteFilePaths.Count, PackageInstallProgressState.WriteSession));

            int i = 0, count = 0;
            IEnumerable<Task> tasks = splitRemoteFilePaths.Select(async (splitRemoteFilePath) =>
            {
                try
                {
                    await WriteInstallSessionAsync(session, $"splitapp{i++}", splitRemoteFilePath, cancellationToken);
                    InstallProgressChanged?.Invoke(this, new InstallProgress(++count, splitRemoteFilePaths.Count, PackageInstallProgressState.WriteSession));
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

            InstallProgressChanged?.Invoke(this, new InstallProgress(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            await client.ExecuteShellCommandAsync(Device, $"pm install-commit {session}", receiver, cancellationToken);

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
            progress(localFilePath, new SyncProgressChangedEventArgs(0, 0));

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
                        sync.SyncProgressChanged += (sender, e) => progress(localFilePath, e);
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
            finally
            {
                progress(true, new SyncProgressChangedEventArgs(0, 0));
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