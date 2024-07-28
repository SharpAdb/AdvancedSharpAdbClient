// <copyright file="ForwardSpec.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Represents an adb forward specification as used by the various adb port forwarding functions.
    /// </summary>
    [DebuggerDisplay($"{nameof(ForwardSpec)} \\{{ {nameof(Protocol)} = {{{nameof(Protocol)}}}, {nameof(Port)} = {{{nameof(Port)}}}, {nameof(SocketName)} = {{{nameof(SocketName)}}}, {nameof(ProcessId)} = {{{nameof(ProcessId)}}} }}")]
    public readonly struct ForwardSpec : IEquatable<ForwardSpec>
    {
        /// <summary>
        /// Provides a mapping between a <see cref="string"/> and a <see cref="ForwardProtocol"/>
        /// value, used for the <see cref="string"/> representation of the <see cref="ForwardSpec"/>
        /// class.
        /// </summary>
        private static readonly Dictionary<string, ForwardProtocol> Mappings = new(6, StringComparer.OrdinalIgnoreCase)
        {
            { "tcp", ForwardProtocol.Tcp },
            { "localabstract", ForwardProtocol.LocalAbstract },
            { "localreserved", ForwardProtocol.LocalReserved },
            { "localfilesystem", ForwardProtocol.LocalFilesystem },
            { "dev", ForwardProtocol.Device },
            { "jdwp", ForwardProtocol.JavaDebugWireProtocol }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardSpec"/> struct from its <see cref="string"/> representation.
        /// </summary>
        /// <param name="spec">A <see cref="string"/> which represents a <see cref="ForwardSpec"/>.</param>
        public ForwardSpec(string spec)
        {
            ExceptionExtensions.ThrowIfNull(spec);

            string[] parts = spec.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(spec));
            }

            if (!Mappings.TryGetValue(parts[0], out ForwardProtocol value))
            {
                throw new ArgumentOutOfRangeException(nameof(spec));
            }

            ForwardProtocol protocol = value;
            Protocol = protocol;

            bool isInt = int.TryParse(parts[1], out int intValue);

            switch (protocol)
            {
                case ForwardProtocol.JavaDebugWireProtocol:
                    if (!isInt)
                    {
                        throw new ArgumentOutOfRangeException(nameof(spec));
                    }
                    ProcessId = intValue;
                    break;

                case ForwardProtocol.Tcp:
                    if (!isInt)
                    {
                        throw new ArgumentOutOfRangeException(nameof(spec));
                    }
                    Port = intValue;
                    break;

                case ForwardProtocol.LocalAbstract
                    or ForwardProtocol.LocalFilesystem
                    or ForwardProtocol.LocalReserved
                    or ForwardProtocol.Device:
                    SocketName = parts[1];
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the protocol which is being forwarded.
        /// </summary>
        public ForwardProtocol Protocol { get; init; }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.Tcp"/>, the port
        /// number of the port being forwarded.
        /// </summary>
        public int Port { get; init; }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.LocalAbstract"/>,
        /// <see cref="ForwardProtocol.LocalReserved"/> or <see cref="ForwardProtocol.LocalFilesystem"/>,
        /// the Unix domain socket name of the socket being forwarded.
        /// </summary>
        public string? SocketName { get; init; }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.JavaDebugWireProtocol"/>,
        /// the process id of the process to which the debugger is attached.
        /// </summary>
        public int ProcessId { get; init; }

        /// <summary>
        /// Creates a <see cref="ForwardSpec"/> from its <see cref="string"/> representation.
        /// </summary>
        /// <param name="spec">A <see cref="string"/> which represents a <see cref="ForwardSpec"/>.</param>
        /// <returns>A <see cref="ForwardSpec"/> which represents <paramref name="spec"/>.</returns>
        public static ForwardSpec Parse(string spec) => new(spec);

        /// <inheritdoc/>
        public override string ToString()
        {
            ForwardProtocol protocol = Protocol;
            string protocolString = Mappings.FirstOrDefault(v => v.Value == protocol).Key;
            return protocol switch
            {
                ForwardProtocol.JavaDebugWireProtocol => $"{protocolString}:{ProcessId}",
                ForwardProtocol.Tcp => $"{protocolString}:{Port}",
                ForwardProtocol.LocalAbstract
                or ForwardProtocol.LocalFilesystem
                or ForwardProtocol.LocalReserved
                or ForwardProtocol.Device => $"{protocolString}:{SocketName}",
                _ => string.Empty,
            };
        }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is ForwardSpec other && Equals(other);

        /// <inheritdoc/>
        public bool Equals(ForwardSpec other) =>
            Protocol == other.Protocol
            && Protocol switch
            {
                ForwardProtocol.JavaDebugWireProtocol => ProcessId == other.ProcessId,
                ForwardProtocol.Tcp => Port == other.Port,
                ForwardProtocol.LocalAbstract
                or ForwardProtocol.LocalFilesystem
                or ForwardProtocol.LocalReserved
                or ForwardProtocol.Device => string.Equals(SocketName, other.SocketName),
                _ => false,
            };

        /// <summary>
        /// Tests whether two <see cref='ForwardSpec'/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref='ForwardSpec'/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref='ForwardSpec'/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="ForwardSpec"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(ForwardSpec left, ForwardSpec right) => left.Equals(right);

        /// <summary>
        /// Tests whether two <see cref='ForwardSpec'/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref='ForwardSpec'/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref='ForwardSpec'/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="ForwardSpec"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(ForwardSpec left, ForwardSpec right) => !left.Equals(right);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Protocol, Port, ProcessId, SocketName);
    }
}
