namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents the state of the installation for <see cref="PackageManager.ProgressHandler"/>.
    /// </summary>
    public class InstallProgress
    {
        /// <summary>
        /// State of the installation.
        /// </summary>
        public PackageInstallProgressState State { get; }

        /// <summary>
        /// Number of packages which is finished operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/> and <see cref="PackageInstallProgressState.WriteSession"/> state.
        /// </summary>
        public int PackageFinished { get; }

        /// <summary>
        /// Number of packages required for this operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/> and <see cref="PackageInstallProgressState.WriteSession"/> state.
        /// </summary>
        public int PackageRequired { get; }

        /// <summary>
        /// Upload percentage completed.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        public double UploadProgress { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgress"/> class.
        /// </summary>
        public InstallProgress(PackageInstallProgressState state)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgress"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        public InstallProgress(int packageUploaded, int packageRequired, double uploadProgress) : this(PackageInstallProgressState.Uploading)
        {
            PackageFinished = packageUploaded;
            PackageRequired = packageRequired;
            UploadProgress = uploadProgress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgress"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/> and <see cref="PackageInstallProgressState.WriteSession"/> state.
        /// </summary>
        public InstallProgress(int packageCleaned, int packageRequired, PackageInstallProgressState state) : this(state)
        {
            PackageFinished = packageCleaned;
            PackageRequired = packageRequired;
        }
    }
}
