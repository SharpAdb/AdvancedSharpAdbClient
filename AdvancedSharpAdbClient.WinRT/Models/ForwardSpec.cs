// <copyright file="ForwardSpec.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Represents an adb forward specification as used by the various adb port forwarding functions.
    /// </summary>
    public sealed class ForwardSpec
    {
        internal readonly AdvancedSharpAdbClient.ForwardSpec forwardSpec;

        /// <summary>
        /// Gets or sets the protocol which is being forwarded.
        /// </summary>
        public ForwardProtocol Protocol
        {
            get => (ForwardProtocol)forwardSpec.Protocol;
            set => forwardSpec.Protocol = (AdvancedSharpAdbClient.ForwardProtocol)value;
        }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.Tcp"/>, the port
        /// number of the port being forwarded.
        /// </summary>
        public int Port
        {
            get => forwardSpec.Port;
            set => forwardSpec.Port = value;
        }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.LocalAbstract"/>,
        /// <see cref="ForwardProtocol.LocalReserved"/> or <see cref="ForwardProtocol.LocalFilesystem"/>,
        /// the Unix domain socket name of the socket being forwarded.
        /// </summary>
        public string SocketName
        {
            get => forwardSpec.SocketName;
            set => forwardSpec.SocketName = value;
        }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.JavaDebugWireProtocol"/>,
        /// the process id of the process to which the debugger is attached.
        /// </summary>
        public int ProcessId
        {
            get => forwardSpec.ProcessId;
            set => forwardSpec.ProcessId = value;
        }

        /// <summary>
        /// Creates a <see cref="ForwardSpec"/> from its <see cref="string"/> representation.
        /// </summary>
        /// <param name="spec">A <see cref="string"/> which represents a <see cref="ForwardSpec"/>.</param>
        /// <returns>A <see cref="ForwardSpec"/> which represents <paramref name="spec"/>.</returns>
        public static ForwardSpec Parse(string spec) => GetForwardSpec(AdvancedSharpAdbClient.ForwardSpec.Parse(spec));

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardSpec"/> class.
        /// </summary>
        public ForwardSpec() => forwardSpec = new();

        internal ForwardSpec(AdvancedSharpAdbClient.ForwardSpec forwardSpec) => this.forwardSpec = forwardSpec;

        internal static ForwardSpec GetForwardSpec(AdvancedSharpAdbClient.ForwardSpec forwardSpec) => new(forwardSpec);

        /// <inheritdoc/>
        public override string ToString() => forwardSpec.ToString();

        /// <inheritdoc/>
        public override int GetHashCode() => forwardSpec.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals(object obj) => forwardSpec.Equals(obj);
    }
}
