// <copyright file="PackageManager.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Allows you to get information about packages that are installed on a device.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
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
        private readonly ILogger<PackageManager> logger;

        /// <summary>
        /// A function which returns a new instance of a class
        /// that implements the <see cref="ISyncService"/> interface,
        /// that can be used to transfer files to and from a given device.
        /// </summary>
        protected readonly Func<IAdbClient, DeviceData, ISyncService> SyncServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManager"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.</param>
        /// <param name="device">The device on which to look for packages.</param>
        /// <param name="arguments">The arguments to pass to <c>pm list packages</c>.</param>
        public PackageManager(IAdbClient client, DeviceData device, params string[] arguments) : this(client, device, null, false, null, arguments)
        {
        }

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
            AdbClient = client ?? throw new ArgumentNullException(nameof(client));
            Device = DeviceData.EnsureDevice(ref device);
            Packages = [];
            Arguments = arguments;

            SyncServiceFactory = syncServiceFactory ?? Factories.SyncServiceFactory;

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
        public IAdbClient AdbClient { get; init; }

        /// <summary>
        /// Refreshes the packages.
        /// </summary>
        public void RefreshPackages()
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
            AdbClient.ExecuteShellCommand(Device, cmd, pmr);
        }

        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public void InstallPackage(string packageFilePath, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            ValidateDevice();

            void OnSyncProgressChanged(string? sender, SyncProgressChangedEventArgs args) =>
                callback?.Invoke(new InstallProgressEventArgs(sender is null ? 1 : 0, 1, args.ProgressPercentage));

            string remoteFilePath = SyncPackageToDevice(packageFilePath, OnSyncProgressChanged);

            InstallRemotePackage(remoteFilePath, callback, arguments);

            callback?.Invoke(new InstallProgressEventArgs(0, 1, PackageInstallProgressState.PostInstall));
            RemoveRemotePackage(remoteFilePath);
            callback?.Invoke(new InstallProgressEventArgs(1, 1, PackageInstallProgressState.PostInstall));

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public void InstallRemotePackage(string remoteFilePath, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
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
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultiplePackage(string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            ValidateDevice();

            int splitPackageFileCount = splitPackageFilePaths.Count();

            void OnMainSyncProgressChanged(string? sender, SyncProgressChangedEventArgs args) =>
                callback?.Invoke(new InstallProgressEventArgs(sender is null ? 1 : 0, splitPackageFileCount + 1, args.ProgressPercentage / 2));

            string baseRemoteFilePath = SyncPackageToDevice(basePackageFilePath, OnMainSyncProgressChanged);

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
                        status[path] = args.ProgressPercentage;
                    }
                    callback?.Invoke(new InstallProgressEventArgs(progressCount, splitPackageFileCount + 1, (status.Values.Select(x => x / splitPackageFileCount).Sum() + 100) / 2));
                }
            }

            int i = 0;
            string[] splitRemoteFilePaths = new string[splitPackageFileCount];
            foreach (string splitPackageFilePath in splitPackageFilePaths)
            {
                splitRemoteFilePaths[i++] = SyncPackageToDevice(splitPackageFilePath, OnSplitSyncProgressChanged);
            }

            InstallMultipleRemotePackage(baseRemoteFilePath, splitRemoteFilePaths, callback, arguments);

            int count = 0;
            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                RemoveRemotePackage(splitRemoteFilePath);
                callback?.Invoke(new InstallProgressEventArgs(++count, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));
            }

            RemoveRemotePackage(baseRemoteFilePath);
            callback?.Invoke(new InstallProgressEventArgs(++count, splitRemoteFilePaths.Length + 1, PackageInstallProgressState.PostInstall));

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultiplePackage(IEnumerable<string> splitPackageFilePaths, string packageName, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
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
                        status[path] = args.ProgressPercentage;
                    }
                    callback?.Invoke(new InstallProgressEventArgs(progressCount, splitPackageFileCount, status.Values.Select(x => x / splitPackageFileCount).Sum()));
                }
            }

            int i = 0;
            string[] splitRemoteFilePaths = new string[splitPackageFileCount];
            foreach (string splitPackageFilePath in splitPackageFilePaths)
            {
                splitRemoteFilePaths[i++] = SyncPackageToDevice(splitPackageFilePath, OnSyncProgressChanged);
            }

            InstallMultipleRemotePackage(splitRemoteFilePaths, packageName, callback, arguments);

            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFilePaths.Length, PackageInstallProgressState.PostInstall));
            int count = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                RemoveRemotePackage(splitRemoteFilePath);
                callback?.Invoke(new InstallProgressEventArgs(++count, splitRemoteFilePaths.Length, PackageInstallProgressState.PostInstall));
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <summary>
        /// Installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="baseRemoteFilePath">The absolute base app file path to package file on device.</param>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultipleRemotePackage(string baseRemoteFilePath, IEnumerable<string> splitRemoteFilePaths, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = CreateInstallSession(arguments: arguments);

            int splitRemoteFileCount = splitRemoteFilePaths.Count();

            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFileCount + 1, PackageInstallProgressState.WriteSession));

            WriteInstallSession(session, "base", baseRemoteFilePath);

            callback?.Invoke(new InstallProgressEventArgs(1, splitRemoteFileCount + 1, PackageInstallProgressState.WriteSession));

            int count = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                WriteInstallSession(session, $"split{count++}", splitRemoteFilePath);
                callback?.Invoke(new InstallProgressEventArgs(count, splitRemoteFileCount + 1, PackageInstallProgressState.WriteSession));
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));

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
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultipleRemotePackage(IEnumerable<string> splitRemoteFilePaths, string packageName, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));

            ValidateDevice();

            string session = CreateInstallSession(packageName, arguments);

            int splitRemoteFileCount = splitRemoteFilePaths.Count();

            callback?.Invoke(new InstallProgressEventArgs(0, splitRemoteFileCount, PackageInstallProgressState.WriteSession));

            int count = 0;
            foreach (string splitRemoteFilePath in splitRemoteFilePaths)
            {
                WriteInstallSession(session, $"split{count++}", splitRemoteFilePath);
                callback?.Invoke(new InstallProgressEventArgs(count, splitRemoteFileCount, PackageInstallProgressState.WriteSession));
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));

            InstallOutputReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, $"pm install-commit {session}", receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

#if !NETFRAMEWORK || NET40_OR_GREATER
        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">The absolute file system path to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public void InstallPackage(string packageFilePath, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            InstallPackage(packageFilePath, progress.AsAction(), arguments);

        /// <summary>
        /// Installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public void InstallRemotePackage(string remoteFilePath, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            InstallRemotePackage(remoteFilePath, progress.AsAction(), arguments);

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="basePackageFilePath">The absolute base app file system path to file on local host to install.</param>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultiplePackage(string basePackageFilePath, IEnumerable<string> splitPackageFilePaths, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            InstallMultiplePackage(basePackageFilePath, splitPackageFilePaths, progress.AsAction(), arguments);

        /// <summary>
        /// Installs Android multiple application on device.
        /// </summary>
        /// <param name="splitPackageFilePaths">The absolute split app file system paths to file on local host to install.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultiplePackage(IEnumerable<string> splitPackageFilePaths, string packageName, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            InstallMultiplePackage(splitPackageFilePaths, packageName, progress.AsAction(), arguments);

        /// <summary>
        /// Installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="baseRemoteFilePath">The absolute base app file path to package file on device.</param>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultipleRemotePackage(string baseRemoteFilePath, IEnumerable<string> splitRemoteFilePaths, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            InstallMultipleRemotePackage(baseRemoteFilePath, splitRemoteFilePaths, progress.AsAction(), arguments);

        /// <summary>
        /// Installs the multiple application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="splitRemoteFilePaths">The absolute split app file paths to package file on device.</param>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        public void InstallMultipleRemotePackage(IEnumerable<string> splitRemoteFilePaths, string packageName, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            InstallMultipleRemotePackage(splitRemoteFilePaths, packageName, progress.AsAction(), arguments);
#endif

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="arguments">The arguments to pass to <c>pm uninstall</c>.</param>
        public void UninstallPackage(string packageName, params string[] arguments)
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
        public VersionInfo GetVersionInfo(string packageName)
        {
            ValidateDevice();

            VersionInfoReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, $"dumpsys package {packageName}", receiver);
            return receiver.VersionInfo;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            new StringBuilder(nameof(PackageManager))
                .Append(" { ")
                .Append(nameof(Device))
                .Append(" = ")
                .Append(Device)
                .Append(", ")
                .Append(nameof(AdbClient))
                .Append(" = ")
                .Append(AdbClient)
                .Append(" }")
                .ToString();

        /// <summary>
        /// Like "install", but starts an install session.
        /// </summary>
        /// <param name="packageName">The absolute package name of the base app.</param>
        /// <param name="arguments">The arguments to pass to <c>pm install-create</c>.</param>
        /// <returns>Session ID.</returns>
        protected string CreateInstallSession(string? packageName = null, params string[] arguments)
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
        protected void WriteInstallSession(string session, string apkName, string path)
        {
            ValidateDevice();

            InstallOutputReceiver receiver = new();
            AdbClient.ExecuteShellCommand(Device, $"pm install-write {session} {apkName}.apk \"{path}\"", receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A read-only <see cref="Stream"/> on the specified path.</returns>
        protected virtual Stream GetFileStream(string path) =>
#if WINDOWS_UWP
            StorageFile.GetFileFromPathAsync(path).AwaitByTaskCompleteSource().OpenStreamForReadAsync().AwaitByTaskCompleteSource();
#else
            File.OpenRead(path);
#endif

        /// <summary>
        /// Pushes a file to device
        /// </summary>
        /// <param name="localFilePath">The absolute path to file on local host.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.</param>
        /// <returns>Destination path on device for file.</returns>
        /// <exception cref="IOException">If fatal error occurred when pushing file.</exception>
        protected virtual string SyncPackageToDevice(string localFilePath, Action<string?, SyncProgressChangedEventArgs>? callback)
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
                    using Stream stream = GetFileStream(localFilePath);

                    logger.LogDebug("Uploading file onto device '{0}'", Device.Serial);

                    Action<SyncProgressChangedEventArgs>? progress = callback == null ? null : args => callback.Invoke(localFilePath, args);

                    // As C# can't use octal, the octal literal 666 (rw-Permission) is here converted to decimal (438)
                    sync.Push(stream, remoteFilePath, UnixFileStatus.DefaultFileMode, File.GetLastWriteTime(localFilePath), progress, false);
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
        /// Remove a file from device.
        /// </summary>
        /// <param name="remoteFilePath">Path on device of file to remove.</param>
        /// <exception cref="IOException">If file removal failed.</exception>
        protected void RemoveRemotePackage(string remoteFilePath)
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
        /// Validates the device is online.
        /// </summary>
        protected void ValidateDevice()
        {
            if (Device.State != DeviceState.Online)
            {
                throw new AdbException("Device is offline");
            }
        }
    }
}
