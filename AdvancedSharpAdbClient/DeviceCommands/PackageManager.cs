// <copyright file="PackageManager.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        /// The logger to use when logging messages.
        /// </summary>
        protected readonly ILogger<PackageManager> logger;

        /// <summary>
        /// A function which returns a new instance of a class
        /// that implements the <see cref="ISyncService"/> interface,
        /// that can be used to transfer files to and from a given device.
        /// </summary>
        protected readonly Func<IAdbClient, DeviceData, ISyncService> syncServiceFactory;

        /// <summary>
        /// Occurs when there is a change in the status of the installing.
        /// </summary>
        public event EventHandler<InstallProgressEventArgs>? InstallProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManager"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.</param>
        /// <param name="device">The device on which to look for packages.</param>
        /// <param name="syncServiceFactory">A function which returns a new instance of a class
        /// that implements the <see cref="ISyncService"/> interface,
        /// that can be used to transfer files to and from a given device.</param>
        /// <param name="skipInit">A value indicating whether to skip the initial refresh of the package list or not.
        /// Used mainly by unit tests.</param>
        /// <param name="logger">The logger to use when logging.</param>
        /// <param name="arguments">The arguments to pass to <c>pm list packages</c>.</param>
        public PackageManager(IAdbClient client, DeviceData device, Func<IAdbClient, DeviceData, ISyncService>? syncServiceFactory = null, bool skipInit = false, ILogger<PackageManager>? logger = null, params string[] arguments)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            Packages = [];
            AdbClient = client ?? throw new ArgumentNullException(nameof(client));
            Arguments = arguments;

            this.syncServiceFactory = syncServiceFactory ?? Factories.SyncServiceFactory;

            if (!skipInit)
            {
                RefreshPackages();
            }

            this.logger = logger ?? LoggerProvider.CreateLogger<PackageManager>();
        }

        /// <summary>
        /// Gets or sets a value to pass to <c>pm list packages</c> when list packages.
        /// </summary>
        /// <remarks>
        /// <list type="ordered">
        ///   <item>
        ///     <c>-a</c>: all known packages (but excluding APEXes)
        ///   </item>
        ///   <item>
        ///     <c>-d</c>: filter to only show disabled packages
        ///   </item>
        ///   <item>
        ///     <c>-e</c>: filter to only show enabled packages
        ///   </item>
        ///   <item>
        ///     <c>-s</c>: filter to only show system packages
        ///   </item>
        ///   <item>
        ///     <c>-3</c>: filter to only show third party packages
        ///   </item>
        ///   <item>
        ///     <c>-i</c>: ignored (used for compatibility with older releases)
        ///   </item>
        ///   <item>
        ///     <c>-u</c>: also include uninstalled packages
        ///   </item>
        /// </list>
        /// </remarks>
        public string[] Arguments { get; set; }

        /// <summary>
        /// Gets the list of packages currently installed on the device. They key is the name of the package;
        /// the value the package path.
        /// </summary>
        public Dictionary<string, string> Packages { get; private set; }

        /// <summary>
        /// Gets the device.
        /// </summary>
        public DeviceData Device { get; init; }

        /// <summary>
        /// The <see cref="IAdbClient"/> to use when communicating with the device.
        /// </summary>
        protected IAdbClient AdbClient { get; init; }

        /// <summary>
        /// Refreshes the packages.
        /// </summary>
        public virtual void RefreshPackages()
        {
            ValidateDevice();

            StringBuilder requestBuilder = new StringBuilder().Append(ListFull);

            if (Arguments != null)
            {
                foreach (string argument in Arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            string cmd = requestBuilder.ToString();
            PackageManagerReceiver pmr = new(this);
            AdbClient.ExecuteShellCommand(Device, cmd, pmr);
        }

        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public virtual void InstallPackage(string packageFilePath, params string[] arguments)
        {
            ValidateDevice();

            string remoteFilePath = SyncPackageToDevice(packageFilePath, OnSyncProgressChanged);

            InstallRemotePackage(remoteFilePath, arguments);

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
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public virtual void InstallRemotePackage(string remoteFilePath, params string[] arguments)
        {
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            ValidateDevice();

            StringBuilder requestBuilder = new StringBuilder().Append("pm install");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            _ = requestBuilder.AppendFormat(" \"{0}\"", remoteFilePath);

            string cmd = requestBuilder.ToString();
            InstallOutputReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, cmd, receiver);

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
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public virtual void InstallMultiplePackage(string basePackageFilePath, IList<string> splitPackageFilePaths, params string[] arguments)
        {
            ValidateDevice();

            void OnMainSyncProgressChanged(object sender, SyncProgressChangedEventArgs args) =>
                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(sender is true ? 1 : 0, splitPackageFilePaths.Count + 1, args.ProgressPercentage / 2));

            string baseRemoteFilePath = SyncPackageToDevice(basePackageFilePath, OnMainSyncProgressChanged);

            Dictionary<string, double> progress = new(splitPackageFilePaths.Count);
            void OnSplitSyncProgressChanged(object sender, SyncProgressChangedEventArgs args)
            {
                lock (progress)
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
                    double present = progress.Values.Select(x => x / splitPackageFilePaths.Count / 2).Sum();
                    InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(count, splitPackageFilePaths.Count + 1, present));
                }
            }

            string[] splitRemoteFilePaths = new string[splitPackageFilePaths.Count];
            for (int i = 0; i < splitPackageFilePaths.Count; i++)
            {
                splitRemoteFilePaths[i] = SyncPackageToDevice(splitPackageFilePaths[i], OnSplitSyncProgressChanged);
            }

            InstallMultipleRemotePackage(baseRemoteFilePath, splitRemoteFilePaths, arguments);

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
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public virtual void InstallMultiplePackage(IList<string> splitPackageFilePaths, string packageName, params string[] arguments)
        {
            ValidateDevice();

            Dictionary<string, double> progress = new(splitPackageFilePaths.Count);
            void OnSyncProgressChanged(object sender, SyncProgressChangedEventArgs args)
            {
                lock (progress)
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
                    double present = progress.Values.Select(x => x / splitPackageFilePaths.Count).Sum();
                    InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(count, splitPackageFilePaths.Count, present));
                }
            }

            string[] splitRemoteFilePaths = new string[splitPackageFilePaths.Count];
            for (int i = 0; i < splitPackageFilePaths.Count; i++)
            {
                splitRemoteFilePaths[i] = SyncPackageToDevice(splitPackageFilePaths[i], OnSyncProgressChanged);
            }

            InstallMultipleRemotePackage(splitRemoteFilePaths, packageName, arguments);

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
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public virtual void InstallMultipleRemotePackage(string baseRemoteFilePath, ICollection<string> splitRemoteFilePaths, params string[] arguments)
        {
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = CreateInstallSession(arguments: arguments);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(0, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));

            WriteInstallSession(session, "base", baseRemoteFilePath);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(1, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));

            int count = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                WriteInstallSession(session, $"split{count++}", splitRemoteFilePath);
                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(count, splitRemoteFilePaths.Count + 1, PackageInstallProgressState.WriteSession));
            }

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, $"pm install-commit {session}", receiver);

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
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public virtual void InstallMultipleRemotePackage(ICollection<string> splitRemoteFilePaths, string packageName, params string[] arguments)
        {
            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = CreateInstallSession(packageName, arguments);

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(0, splitRemoteFilePaths.Count, PackageInstallProgressState.WriteSession));

            int count = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                WriteInstallSession(session, $"split{count++}", splitRemoteFilePath);
                InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(count, splitRemoteFilePaths.Count, PackageInstallProgressState.WriteSession));
            }

            InstallProgressChanged?.Invoke(this, new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, $"pm install-commit {session}", receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="arguments">The arguments to pass to <c>pm uninstall</c>.</param>
        public virtual void UninstallPackage(string packageName, params string[] arguments)
        {
            ValidateDevice();

            StringBuilder requestBuilder = new StringBuilder().Append("pm uninstall");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            _ = requestBuilder.AppendFormat(" {0}", packageName);

            string cmd = requestBuilder.ToString();
            InstallOutputReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, cmd, receiver);
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
            AdbClient.ExecuteShellCommand(Device, $"dumpsys package {packageName}", receiver);
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
        protected virtual string SyncPackageToDevice(string localFilePath, Action<object, SyncProgressChangedEventArgs>? progress)
        {
            progress?.Invoke(localFilePath, new SyncProgressChangedEventArgs(0, 0));

            ValidateDevice();

            try
            {
                string packageFileName = Path.GetFileName(localFilePath);

                // only root has access to /data/local/tmp/... not sure how adb does it then...
                // workitem: 16823
                // workitem: 19711
                string remoteFilePath = LinuxPath.Combine(TempInstallationDirectory, packageFileName);

                logger.LogDebug("Uploading {0} onto device '{1}'", packageFileName, Device.Serial);

                using (ISyncService sync = syncServiceFactory(AdbClient, Device))
                {
                    if (progress != null)
                    {
                        sync.SyncProgressChanged += (sender, e) => progress(localFilePath, e);
                    }

                    using FileStream stream = File.OpenRead(localFilePath);

                    logger.LogDebug("Uploading file onto device '{0}'", Device.Serial);

                    // As C# can't use octal, the octal literal 666 (rw-Permission) is here converted to decimal (438)
                    sync.Push(stream, remoteFilePath, 438, File.GetLastWriteTime(localFilePath), null, false);
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
                progress?.Invoke(true, new SyncProgressChangedEventArgs(0, 0));
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
                AdbClient.ExecuteShellCommand(Device, $"rm \"{remoteFilePath}\"");
            }
            catch (IOException e)
            {
                logger.LogError(e, "Failed to delete temporary package: {0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Like "install", but starts an install session.
        /// </summary>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>Session ID.</returns>
        protected virtual string CreateInstallSession(string? packageName = null, params string[] arguments)
        {
            ValidateDevice();

            StringBuilder requestBuilder = new StringBuilder().Append("pm install-create");

            if (!StringExtensions.IsNullOrWhiteSpace(packageName))
            {
                _ = requestBuilder.AppendFormat(" -p {0}", packageName);
            }

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            string cmd = requestBuilder.ToString();
            InstallOutputReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, cmd, receiver);

            if (string.IsNullOrEmpty(receiver.SuccessMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }

            string result = receiver.SuccessMessage ?? throw new AdbException($"The {nameof(result)} of {nameof(CreateInstallSession)} is null.");
            int arr = result.IndexOf(']') - 1 - result.IndexOf('[');
            string session = result.Substring(result.IndexOf('[') + 1, arr);

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
            AdbClient.ExecuteShellCommand(Device, $"pm install-write {session} {apkName}.apk \"{path}\"", receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }
    }
}
