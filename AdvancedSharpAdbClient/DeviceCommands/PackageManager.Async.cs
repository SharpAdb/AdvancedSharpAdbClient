#if HAS_TASK
// <copyright file="PackageManager.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    public partial class PackageManager
    {
        /// <summary>
        /// Asynchronously refreshes the packages.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task RefreshPackagesAsync(CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            StringBuilder requestBuilder = new(ListFull);

            if (Arguments != null)
            {
                foreach (string argument in Arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            string cmd = requestBuilder.ToString();
            PackageManagerReceiver pmr = new(this);
            return AdbClient.ExecuteShellCommandAsync(Device, cmd, pmr, cancellationToken);
        }

        /// <summary>
        /// Asynchronously installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallPackageAsync(string packageFilePath, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            ValidateDevice();

            void OnSyncProgressChanged(string? sender, SyncProgressChangedEventArgs args) =>
                callback?.Invoke(new InstallProgressEventArgs(sender is null ? 1 : 0, 1, args.ProgressPercentage));

            string remoteFilePath = await SyncPackageToDeviceAsync(packageFilePath, OnSyncProgressChanged, cancellationToken).ConfigureAwait(false);

            await InstallRemotePackageAsync(remoteFilePath, callback, cancellationToken, arguments).ConfigureAwait(false);

            callback?.Invoke(new InstallProgressEventArgs(0, 1, PackageInstallProgressState.PostInstall));
            await RemoveRemotePackageAsync(remoteFilePath, cancellationToken).ConfigureAwait(false);
            callback?.Invoke(new InstallProgressEventArgs(1, 1, PackageInstallProgressState.PostInstall));

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Asynchronously installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallRemotePackageAsync(string remoteFilePath, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            ValidateDevice();

            StringBuilder requestBuilder = new("pm install");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            _ = requestBuilder.Append(" \"").Append(remoteFilePath).Append('"');

            string cmd = requestBuilder.ToString();
            InstallOutputReceiver receiver = new();
            await AdbClient.ExecuteShellCommandAsync(Device, cmd, receiver, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultiplePackageAsync(string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            ValidateDevice();

            int splitPackageFileCount = splitPackageFilePaths.Count();

            void OnMainSyncProgressChanged(string? sender, SyncProgressChangedEventArgs args) =>
                callback?.Invoke(new InstallProgressEventArgs(sender is null ? 1 : 0, splitPackageFileCount + 1, args.ProgressPercentage / 2));

            string baseRemoteFilePath = await SyncPackageToDeviceAsync(basePackageFilePath, OnMainSyncProgressChanged, cancellationToken).ConfigureAwait(false);

            int progressCount = 1;
            Dictionary<string, double> status = new(splitPackageFileCount);
            void OnSplitSyncProgressChanged(string? sender, SyncProgressChangedEventArgs args)
            {
                lock (status)
                {
                    if (sender is null)
                    {
                        progressCount++;
                    }
                    else if (sender is string path)
                    {
                        // Note: The progress may be less than the previous progress when async.
                        if (status.TryGetValue(path, out double oldValue)
                            && oldValue > args.ProgressPercentage)
                        {
                            return;
                        }
                        status[path] = args.ProgressPercentage;
                    }
                    callback?.Invoke(new InstallProgressEventArgs(progressCount, splitPackageFileCount + 1, (status.Values.Select(x => x / splitPackageFileCount).Sum() + 100) / 2));
                }
            }

            string[] splitRemoteFilePaths = await splitPackageFilePaths.Select(x => SyncPackageToDeviceAsync(x, OnSplitSyncProgressChanged, cancellationToken)).WhenAll().ConfigureAwait(false);

            if (splitRemoteFilePaths.Length < splitPackageFileCount)
            {
                throw new PackageInstallationException($"{nameof(SyncPackageToDeviceAsync)} failed. {splitPackageFileCount} should process but only {splitRemoteFilePaths.Length} processed.");
            }

            await InstallMultipleRemotePackageAsync(baseRemoteFilePath, splitRemoteFilePaths, callback, cancellationToken, arguments);

            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));
            int count = 0;
            await splitRemoteFilePaths.Select(async x =>
            {
                count++;
                await RemoveRemotePackageAsync(x, cancellationToken).ConfigureAwait(false);
                callback?.Invoke(new InstallProgressEventArgs(count, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));
            }).WhenAll().ConfigureAwait(false);

            if (count < splitRemoteFilePaths.Length)
            {
                throw new PackageInstallationException($"{nameof(RemoveRemotePackageAsync)} failed. {splitRemoteFilePaths.Length} should process but only {count} processed.");
            }

            await RemoveRemotePackageAsync(baseRemoteFilePath, cancellationToken).ConfigureAwait(false);
            callback?.Invoke(new InstallProgressEventArgs(++count, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultiplePackageAsync(IEnumerable<string> splitPackageFilePaths, string packageName, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            ValidateDevice();

            int splitPackageFileCount = splitPackageFilePaths.Count();

            int progressCount = 0;
            Dictionary<string, double> status = new(splitPackageFileCount);
            void OnSyncProgressChanged(string? sender, SyncProgressChangedEventArgs args)
            {
                lock (status)
                {
                    if (sender is null)
                    {
                        progressCount++;
                    }
                    else if (sender is string path)
                    {
                        // Note: The progress may be less than the previous progress when async.
                        if (status.TryGetValue(path, out double oldValue)
                            && oldValue > args.ProgressPercentage)
                        {
                            return;
                        }
                        status[path] = args.ProgressPercentage;
                    }
                    callback?.Invoke(new InstallProgressEventArgs(progressCount, splitPackageFileCount, status.Values.Select(x => x / splitPackageFileCount).Sum()));
                }
            }

            string[] splitRemoteFilePaths = await splitPackageFilePaths.Select(x => SyncPackageToDeviceAsync(x, OnSyncProgressChanged, cancellationToken)).WhenAll().ConfigureAwait(false);

            if (splitRemoteFilePaths.Length < splitPackageFileCount)
            {
                throw new PackageInstallationException($"{nameof(SyncPackageToDeviceAsync)} failed. {splitPackageFileCount} should process but only {splitRemoteFilePaths.Length} processed.");
            }

            await InstallMultipleRemotePackageAsync(splitRemoteFilePaths, packageName, callback, cancellationToken, arguments);

            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFilePaths.Length, PackageInstallProgressState.PostInstall));
            int count = 0;
            await splitRemoteFilePaths.Select(async x =>
            {
                count++;
                await RemoveRemotePackageAsync(x, cancellationToken).ConfigureAwait(false);
                callback?.Invoke(new InstallProgressEventArgs(count, splitRemoteFilePaths.Length, PackageInstallProgressState.PostInstall));
            }).WhenAll().ConfigureAwait(false);

            if (count < splitRemoteFilePaths.Length)
            {
                throw new PackageInstallationException($"{nameof(RemoveRemotePackageAsync)} failed. {splitRemoteFilePaths.Length} should process but only {count} processed.");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Asynchronously installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="baseRemoteFilePath">The absolute base app file path to package file on device.</param>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultipleRemotePackageAsync(string baseRemoteFilePath, IEnumerable<string> splitRemoteFilePaths, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = await CreateInstallSessionAsync(cancellationToken: cancellationToken, arguments: arguments).ConfigureAwait(false);

            int splitRemoteFileCount = splitRemoteFilePaths.Count();

            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFileCount + 1, PackageInstallProgressState.WriteSession));

            await WriteInstallSessionAsync(session, "base", baseRemoteFilePath, cancellationToken).ConfigureAwait(false);

            callback?.Invoke(new InstallProgressEventArgs(1, splitRemoteFileCount + 1, PackageInstallProgressState.WriteSession));

            int count = 0;
            await splitRemoteFilePaths.Select(async splitRemoteFilePath =>
            {
                await WriteInstallSessionAsync(session, $"split{count++}", splitRemoteFilePath, cancellationToken).ConfigureAwait(false);
                callback?.Invoke(new InstallProgressEventArgs(count, splitRemoteFileCount + 1, PackageInstallProgressState.WriteSession));
            }).WhenAll().ConfigureAwait(false);

            if (count < splitRemoteFileCount)
            {
                throw new PackageInstallationException($"{nameof(WriteInstallSessionAsync)} failed. {splitRemoteFileCount} should process but only {count} processed.");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            await AdbClient.ExecuteShellCommandAsync(Device, $"pm install-commit {session}", receiver, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Asynchronously installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultipleRemotePackageAsync(IEnumerable<string> splitRemoteFilePaths, string packageName, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = await CreateInstallSessionAsync(packageName, cancellationToken, arguments).ConfigureAwait(false);

            int splitRemoteFileCount = splitRemoteFilePaths.Count();

            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFileCount, PackageInstallProgressState.WriteSession));

            int count = 0;
            await splitRemoteFilePaths.Select(async splitRemoteFilePath =>
            {
                await WriteInstallSessionAsync(session, $"split{count++}", splitRemoteFilePath, cancellationToken).ConfigureAwait(false);
                callback?.Invoke(new InstallProgressEventArgs(count, splitRemoteFileCount, PackageInstallProgressState.WriteSession));
            }).WhenAll().ConfigureAwait(false);

            if (count < splitRemoteFileCount)
            {
                throw new PackageInstallationException($"{nameof(WriteInstallSessionAsync)} failed. {splitRemoteFileCount} should process but only {count} processed.");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            await AdbClient.ExecuteShellCommandAsync(Device, $"pm install-commit {session}", receiver, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

#if !NETFRAMEWORK || NET40_OR_GREATER
        /// <summary>
        /// Asynchronously installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallPackageAsync(string packageFilePath, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments) =>
            await InstallPackageAsync(packageFilePath, progress.AsAction(), cancellationToken, arguments).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallRemotePackageAsync(string remoteFilePath, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments) =>
            await InstallRemotePackageAsync(remoteFilePath, progress.AsAction(), cancellationToken, arguments).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultiplePackageAsync(string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments) =>
            await InstallMultiplePackageAsync(basePackageFilePath, splitPackageFilePaths, progress.AsAction(), cancellationToken, arguments).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously installs Android multiple application on device.
        /// </summary>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultiplePackageAsync(IEnumerable<string> splitPackageFilePaths, string packageName, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments) =>
            await InstallMultiplePackageAsync(splitPackageFilePaths, packageName, progress.AsAction(), cancellationToken, arguments).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="baseRemoteFilePath">The absolute base app file path to package file on device.</param>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultipleRemotePackageAsync(string baseRemoteFilePath, IEnumerable<string> splitRemoteFilePaths, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments) =>
            await InstallMultipleRemotePackageAsync(baseRemoteFilePath, splitRemoteFilePaths, progress.AsAction(), cancellationToken, arguments).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task InstallMultipleRemotePackageAsync(IEnumerable<string> splitRemoteFilePaths, string packageName, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments) =>
            await InstallMultipleRemotePackageAsync(splitRemoteFilePaths, packageName, progress.AsAction(), cancellationToken, arguments).ConfigureAwait(false);
#endif

        /// <summary>
        /// Asynchronously uninstalls a package from the device.
        /// </summary>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="arguments">The arguments to pass to <c>pm uninstall</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task UninstallPackageAsync(string packageName, params string[] arguments) =>
            UninstallPackageAsync(packageName, default, arguments);

        /// <summary>
        /// Asynchronously uninstalls a package from the device.
        /// </summary>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm uninstall</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task UninstallPackageAsync(string packageName, CancellationToken cancellationToken, params string[] arguments)
        {
            ValidateDevice();

            StringBuilder requestBuilder = new("pm uninstall");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            _ = requestBuilder.Append(' ').Append(packageName);

            string cmd = requestBuilder.ToString();
            InstallOutputReceiver receiver = new();
            await AdbClient.ExecuteShellCommandAsync(Device, cmd, receiver, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Asynchronously requests the version information from the device.
        /// </summary>
        /// <param name="packageName">The name of the package from which to get the application version.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{VersionInfo}"/> which returns the <see cref="VersionInfo"/> of target application.</returns>
        public async Task<VersionInfo> GetVersionInfoAsync(string packageName, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            VersionInfoReceiver receiver = new();
            await AdbClient.ExecuteShellCommandAsync(Device, $"dumpsys package {packageName}", receiver, cancellationToken).ConfigureAwait(false);
            return receiver.VersionInfo;
        }

        /// <summary>
        /// Like "install", but starts an install session asynchronously.
        /// </summary>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the session ID.</returns>
        protected async Task<string> CreateInstallSessionAsync(string? packageName = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            ValidateDevice();

            StringBuilder requestBuilder = new("pm install-create");

            if (!StringExtensions.IsNullOrWhiteSpace(packageName))
            {
                _ = requestBuilder.Append(" -p ").Append(packageName);
            }

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            string cmd = requestBuilder.ToString();
            InstallOutputReceiver receiver = new();
            await AdbClient.ExecuteShellCommandAsync(Device, cmd, receiver, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(receiver.SuccessMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }

            string result = receiver.SuccessMessage ?? throw new AdbException($"The {nameof(result)} of {nameof(CreateInstallSessionAsync)} is null.");
            int arr = result.IndexOf(']') - 1 - result.IndexOf('[');
            string session = result.Substring(result.IndexOf('[') + 1, arr);

            return session;
        }

        /// <summary>
        /// Asynchronously write an apk into the given install session.
        /// </summary>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="path">The absolute file path to package file on device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected async Task WriteInstallSessionAsync(string session, string apkName, string path, CancellationToken cancellationToken = default)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            await AdbClient.ExecuteShellCommandAsync(Device, $"pm install-write {session} {apkName}.apk \"{path}\"", receiver, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Asynchronously opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Stream}"/> which returns a read-only <see cref="Stream"/> on the specified path.</returns>
        protected virtual Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default) =>
#if WINDOWS_UWP
            StorageFile.GetFileFromPathAsync(path).AsTask(cancellationToken).ContinueWith(x => x.Result.OpenStreamForReadAsync()).Unwrap();
#else
            TaskExExtensions.FromResult<Stream>(File.OpenRead(path));
#endif

        /// <summary>
        /// Asynchronously pushes a file to device
        /// </summary>
        /// <param name="localFilePath">The absolute path to file on local host.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the destination path on device for file.</returns>
        /// <exception cref="IOException">If fatal error occurred when pushing file.</exception>
        protected virtual async Task<string> SyncPackageToDeviceAsync(string localFilePath, Action<string?, SyncProgressChangedEventArgs>? callback, CancellationToken cancellationToken = default)
        {
            callback?.Invoke(localFilePath, new SyncProgressChangedEventArgs(0, 100));

            ValidateDevice();

            try
            {
                string packageFileName = Path.GetFileName(localFilePath);

                // only root has access to /data/local/tmp/... not sure how adb does it then...
                // workitem: 16823
                // workitem: 19711
                string remoteFilePath = LinuxPath.Combine(TempInstallationDirectory, packageFileName);

                logger.LogDebug("Uploading {0} onto device '{1}'", packageFileName, Device.Serial);

                using (ISyncService sync = SyncServiceFactory(AdbClient, Device))
                {
#if COMP_NETSTANDARD2_1
                    await
#endif
                    using Stream stream = await GetFileStreamAsync(localFilePath, cancellationToken).ConfigureAwait(false);

                    logger.LogDebug("Uploading file onto device '{0}'", Device.Serial);

                    Action<SyncProgressChangedEventArgs>? progress = callback == null ? null : args => callback.Invoke(localFilePath, args);

                    // As C# can't use octal, the octal literal 666 (rw-Permission) is here converted to decimal (438)
                    await sync.PushAsync(stream, remoteFilePath, UnixFileStatus.DefaultFileMode, File.GetLastWriteTime(localFilePath), null, cancellationToken).ConfigureAwait(false);
                }

                return remoteFilePath;
            }
            catch (IOException e)
            {
                logger.LogError(e, "Unable to open sync connection! reason: {0}", e.Message);
                throw;
            }
            finally
            {
                callback?.Invoke(null, new SyncProgressChangedEventArgs(100, 100));
            }
        }

        /// <summary>
        /// Asynchronously remove a file from device.
        /// </summary>
        /// <param name="remoteFilePath">Path on device of file to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <exception cref="IOException">If file removal failed.</exception>
        protected async Task RemoveRemotePackageAsync(string remoteFilePath, CancellationToken cancellationToken = default)
        {
            // now we delete the app we synced
            try
            {
                await AdbClient.ExecuteShellCommandAsync(Device, $"rm \"{remoteFilePath}\"", cancellationToken).ConfigureAwait(false);
            }
            catch (IOException e)
            {
                logger.LogError(e, "Failed to delete temporary package: {0}", e.Message);
                throw;
            }
        }
    }
}
#endif