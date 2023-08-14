// <copyright file="PackageManager.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Allows you to get information about packages that are installed on a device.
    /// </summary>
    public partial class PackageManager
    {
        /// <summary>
        /// The path to a temporary directory to use when pushing files to the device.
        /// </summary>
        public const string TempInstallationDirectory = "/data/local/tmp/";

        /// <summary>
        /// The command that list all packages installed on the device.
        /// </summary>
        protected const string ListFull = "pm list packages -f";

        /// <summary>
        /// The command that list all third party packages installed on the device.
        /// </summary>
        protected const string ListThirdPartyOnly = "pm list packages -f -3";

#if HAS_LOGGER
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        protected readonly ILogger<PackageManager> logger;
#endif

        /// <summary>
        /// The <see cref="IAdbClient"/> to use when communicating with the device.
        /// </summary>
        protected readonly IAdbClient client;

        /// <summary>
        /// A function which returns a new instance of a class
        /// that implements the <see cref="ISyncService"/> interface,
        /// that can be used to transfer files to and from a given device.
        /// </summary>
        protected readonly Func<IAdbClient, DeviceData, ISyncService> syncServiceFactory;

        /// <summary>
        /// Occurs when there is a change in the status of the installing.
        /// </summary>
        public event EventHandler<InstallProgressEventArgs> InstallProgressChanged;

#if !HAS_LOGGER
#pragma warning disable CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManager"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.</param>
        /// <param name="device">The device on which to look for packages.</param>
        /// <param name="thirdPartyOnly"><see langword="true"/> to only indicate third party applications;
        /// <see langword="false"/> to also include built-in applications.</param>
        /// <param name="syncServiceFactory">A function which returns a new instance of a class
        /// that implements the <see cref="ISyncService"/> interface,
        /// that can be used to transfer files to and from a given device.</param>
        /// <param name="skipInit">A value indicating whether to skip the initial refresh of the package list or not.
        /// Used mainly by unit tests.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public PackageManager(IAdbClient client, DeviceData device, bool thirdPartyOnly = false, Func<IAdbClient, DeviceData, ISyncService> syncServiceFactory = null, bool skipInit = false
#if HAS_LOGGER
            , ILogger<PackageManager> logger = null
#endif
            )
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            Packages = new Dictionary<string, string>();
            ThirdPartyOnly = thirdPartyOnly;
            this.client = client ?? throw new ArgumentNullException(nameof(client));

            this.syncServiceFactory = syncServiceFactory ?? Factories.SyncServiceFactory;

            if (!skipInit)
            {
                RefreshPackages();
            }

#if HAS_LOGGER
            this.logger = logger ?? NullLogger<PackageManager>.Instance;
#endif
        }
#if !HAS_LOGGER
#pragma warning restore CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif

        /// <summary>
        /// Gets a value indicating whether this package manager only lists third party applications,
        /// or also includes built-in applications.
        /// </summary>
        public bool ThirdPartyOnly { get; private set; }

        /// <summary>
        /// Gets the list of packages currently installed on the device. They key is the name of the package;
        /// the value the package path.
        /// </summary>
        public Dictionary<string, string> Packages { get; private set; }

        /// <summary>
        /// Gets the device.
        /// </summary>
        public DeviceData Device { get; private set; }

        /// <summary>
        /// Refreshes the packages.
        /// </summary>
        public virtual void RefreshPackages()
        {
            ValidateDevice();

            PackageManagerReceiver pmr = new(Device, this);

            if (ThirdPartyOnly)
            {
                client.ExecuteShellCommand(Device, ListThirdPartyOnly, pmr);
            }
            else
            {
                client.ExecuteShellCommand(Device, ListFull, pmr);
            }
        }

        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="reinstall"><see langword="true"/> if re-install of app should be performed; otherwise, <see langword="false"/>.</param>
        public virtual void InstallPackage(string packageFilePath, bool reinstall)
        {
            ValidateDevice();

            string remoteFilePath = SyncPackageToDevice(packageFilePath, OnSyncProgressChanged);

            InstallRemotePackage(remoteFilePath, reinstall);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(0, 1, PackageInstallProgressState.PostInstall));
            RemoveRemotePackage(remoteFilePath);
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(1, 1, PackageInstallProgressState.PostInstall));

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Finished));

            void OnSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) =>
                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(sender is true ? 1 : 0, 1, args.ProgressPercentage));
        }

        /// <summary>
        /// Installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        public virtual void InstallRemotePackage(string remoteFilePath, bool reinstall)
        {
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            ValidateDevice();

            InstallOutputReceiver receiver = new();
            string reinstallSwitch = reinstall ? "-r " : string.Empty;

            string cmd = $"pm install {reinstallSwitch}\"{remoteFilePath}\"";
            client.ExecuteShellCommand(Device, cmd, receiver);

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
        public virtual void InstallMultiplePackage(string basePackageFilePath, IList<string> splitPackageFilePaths, bool reinstall)
        {
            ValidateDevice();

            void OnMainSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) =>
                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(sender is true ? 1 : 0, splitPackageFilePaths.Count + 1, args.ProgressPercentage / 2));

            string baseRemoteFilePath = SyncPackageToDevice(basePackageFilePath, OnMainSyncProgressChanged);

            Dictionary<string, double> progress = new(splitPackageFilePaths.Count);
            void OnSplitSyncProgressChanged(object sender, SyncProgressChangedEventArgs args)
            {
                int count = 1;
                if (sender is string path)
                {
                    progress[path] = args.ProgressPercentage;
                }
                else if (sender is true)
                {
                    count++;
                }

                double present = 0;
                foreach(KeyValuePair<string, double> info in progress)
                {
                    present += (info.Value / splitPackageFilePaths.Count) / 2;
                }

                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(count, splitPackageFilePaths.Count + 1, present));
            }

            string[] splitRemoteFilePaths = new string[splitPackageFilePaths.Count];
            for (int i = 0; i < splitPackageFilePaths.Count; i++)
            {
                splitRemoteFilePaths[i] = SyncPackageToDevice(splitPackageFilePaths[i], OnSplitSyncProgressChanged);
            }

            InstallMultipleRemotePackage(baseRemoteFilePath, splitRemoteFilePaths, reinstall);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(0, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));
            int count = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                RemoveRemotePackage(splitRemoteFilePath);
                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(++count, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));
            }

            RemoveRemotePackage(baseRemoteFilePath);
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(++count, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        public virtual void InstallMultiplePackage(IList<string> splitPackageFilePaths, string packageName, bool reinstall)
        {
            ValidateDevice();

            Dictionary<string, double> progress = new(splitPackageFilePaths.Count);
            void OnSyncProgressChanged(object sender, SyncProgressChangedEventArgs args)
            {
                int count = 1;
                if (sender is string path)
                {
                    progress[path] = args.ProgressPercentage;
                }
                else if (sender is true)
                {
                    count++;
                }

                double present = 0;
                foreach (KeyValuePair<string, double> info in progress)
                {
                    present += info.Value / splitPackageFilePaths.Count;
                }

                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(count, splitPackageFilePaths.Count, present));
            }

            string[] splitRemoteFilePaths = new string[splitPackageFilePaths.Count];
            for (int i = 0; i < splitPackageFilePaths.Count; i++)
            {
                splitRemoteFilePaths[i] = SyncPackageToDevice(splitPackageFilePaths[i], OnSyncProgressChanged);
            }

            InstallMultipleRemotePackage(splitRemoteFilePaths, packageName, reinstall);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(0, splitRemoteFilePaths.Length, PackageInstallProgressState.PostInstall));
            int count = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                RemoveRemotePackage(splitRemoteFilePath);
                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(++count, splitRemoteFilePaths.Length, PackageInstallProgressState.PostInstall));
            }

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="baseRemoteFilePath">The absolute base app file path to package file on device.</param>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="reinstall">Set to <see langword="true"/> if re-install of app should be performed.</param>
        public virtual void InstallMultipleRemotePackage(string baseRemoteFilePath, IList<string> splitRemoteFilePaths, bool reinstall)
        {
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = CreateInstallSession(reinstall);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(0, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));

            WriteInstallSession(session, "base", baseRemoteFilePath);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(1, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));

            int i = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                try
                {
                    WriteInstallSession(session, $"splitapp{i++}", splitRemoteFilePath);
                    InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(i, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            client.ExecuteShellCommand(Device, $"pm install-commit {session}", receiver);

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
        public virtual void InstallMultipleRemotePackage(IList<string> splitRemoteFilePaths, string packageName, bool reinstall)
        {
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = CreateInstallSession(reinstall, packageName);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(0, splitRemoteFilePaths.Count, PackageInstallProgressState.WriteSession));

            int i = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                try
                {
                    WriteInstallSession(session, $"splitapp{i++}", splitRemoteFilePath);
                    InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(i, splitRemoteFilePaths.Count, PackageInstallProgressState.WriteSession));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            client.ExecuteShellCommand(Device, $"pm install-commit {session}", receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="packageName">The name of the package to uninstall.</param>
        public virtual void UninstallPackage(string packageName)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            client.ExecuteShellCommand(Device, $"pm uninstall {packageName}", receiver);
            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Requests the version information from the device.
        /// </summary>
        /// <param name="packageName">The name of the package from which to get the application version.</param>
        /// <returns>The <see cref="VersionInfo"/> of target application.</returns>
        public virtual VersionInfo GetVersionInfo(string packageName)
        {
            ValidateDevice();

            VersionInfoReceiver receiver = new();
            client.ExecuteShellCommand(Device, $"dumpsys package {packageName}", receiver);
            return receiver.VersionInfo;
        }

        /// <summary>
        /// Validates the device is online.
        /// </summary>
        protected void ValidateDevice()
        {
            if (Device.State != DeviceState.Online)
            {
                throw new AdbException("Device is offline");
            }
        }

        /// <summary>
        /// Pushes a file to device
        /// </summary>
        /// <param name="localFilePath">The absolute path to file on local host.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.</param>
        /// <returns>Destination path on device for file.</returns>
        /// <exception cref="IOException">If fatal error occurred when pushing file.</exception>
        protected virtual string SyncPackageToDevice(string localFilePath, Action<object, SyncProgressChangedEventArgs> progress)
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
                    sync.Push(stream, remoteFilePath, 438, File.GetLastWriteTime(localFilePath), null
#if HAS_TASK
                        , CancellationToken.None
#endif
                        );
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
        /// <exception cref="IOException">If file removal failed.</exception>
        protected virtual void RemoveRemotePackage(string remoteFilePath)
        {
            // now we delete the app we synced
            try
            {
                client.ExecuteShellCommand(Device, $"rm \"{remoteFilePath}\"", null);
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
        /// <returns>Session ID.</returns>
        protected virtual string CreateInstallSession(bool reinstall, string packageName = null)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            string reinstallSwitch = reinstall ? " -r" : string.Empty;
            string addon = packageName.IsNullOrWhiteSpace() ? string.Empty : $" -p {packageName}";

            string cmd = $"pm install-create{reinstallSwitch}{addon}";
            client.ExecuteShellCommand(Device, cmd, receiver);

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
        protected virtual void WriteInstallSession(string session, string apkName, string path)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            client.ExecuteShellCommand(Device, $"pm install-write {session} {apkName}.apk \"{path}\"", receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }
    }
}
