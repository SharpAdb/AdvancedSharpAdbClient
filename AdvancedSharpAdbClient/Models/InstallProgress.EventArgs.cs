// <copyright file="InstallProgress.EventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Represents the state of apk installation.
    /// </summary>
    /// <param name="state">The state of the installation.</param>
    [DebuggerDisplay($"{nameof(SyncProgressChangedEventArgs)} \\{{ {nameof(State)} = {{{nameof(State)}}}, {nameof(PackageFinished)} = {{{nameof(PackageFinished)}}}, {nameof(PackageRequired)} = {{{nameof(PackageRequired)}}}, {nameof(UploadProgress)} = {{{nameof(UploadProgress)}}} }}")]
    public sealed class InstallProgressEventArgs(PackageInstallProgressState state) : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgressEventArgs"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        /// <param name="packageUploaded">The number of packages which is finished operation.</param>
        /// <param name="packageRequired">The number of packages required for this operation.</param>
        /// <param name="uploadProgress">Gets the upload percentage (from <see langword="0"/> to <see langword="100"/>) completed.</param>
        public InstallProgressEventArgs(int packageUploaded, int packageRequired, double uploadProgress) : this(PackageInstallProgressState.Uploading)
        {
            PackageFinished = packageUploaded;
            PackageRequired = packageRequired;
            UploadProgress = uploadProgress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgressEventArgs"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/>
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        /// <param name="packageFinished">The number of packages which is finished operation.</param>
        /// <param name="packageRequired">The number of packages required for this operation.</param>
        /// <param name="state">The state of the installation.</param>
        public InstallProgressEventArgs(int packageFinished, int packageRequired, PackageInstallProgressState state) : this(state)
        {
            PackageFinished = packageFinished;
            PackageRequired = packageRequired;
        }

        /// <summary>
        /// Gets the state of the installation.
        /// </summary>
        public PackageInstallProgressState State => state;

        /// <summary>
        /// Gets the number of packages which is finished operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/>,
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        public int PackageFinished { get; init; }

        /// <summary>
        /// Gets the number of packages required for this operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/>,
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        public int PackageRequired { get; init; }

        /// <summary>
        /// Gets the upload percentage (from <see langword="0"/> to <see langword="100"/>) completed.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        public double UploadProgress { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            List<char> split = [];
            string state = State.ToString();
            for (int i = 0; i < state.Length; i++)
            {
                char c = state[i];
                if (i != 0 && c is >= 'A' and <= 'Z')
                {
                    split.Add(' ');
                }
                split.Add(c);
            }

            DefaultInterpolatedStringHandler builder = new(4, 4);
#if NET
            builder.AppendFormatted(CollectionsMarshal.AsSpan(split));
#elif HAS_BUFFERS
            builder.AppendFormatted([.. split]);
#else
            builder.AppendLiteral(new string([.. split]));
#endif

            if (PackageRequired > 0)
            {
                builder.AppendFormatted(' ');
                builder.AppendFormatted(PackageFinished);
                builder.AppendFormatted('/');
                builder.AppendFormatted(PackageRequired);
            }

            if (UploadProgress > 0)
            {
                builder.AppendFormatted(' ');
                builder.AppendFormatted(UploadProgress);
                builder.AppendFormatted('%');
            }

            return builder.ToStringAndClear();
        }
    }
}
