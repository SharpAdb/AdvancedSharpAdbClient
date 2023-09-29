// <copyright file="ForwardSpec.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Represents an adb forward specification as used by the various adb port forwarding functions.
    /// </summary>
    public class ForwardSpec
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
        /// Initializes a new instance of the <see cref="ForwardSpec"/> class.
        /// </summary>
        public ForwardSpec() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardSpec"/> class from its <see cref="string"/> representation.
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
        public ForwardProtocol Protocol { get; set; }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.Tcp"/>, the port
        /// number of the port being forwarded.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.LocalAbstract"/>,
        /// <see cref="ForwardProtocol.LocalReserved"/> or <see cref="ForwardProtocol.LocalFilesystem"/>,
        /// the Unix domain socket name of the socket being forwarded.
        /// </summary>
        public string SocketName { get; set; }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.JavaDebugWireProtocol"/>,
        /// the process id of the process to which the debugger is attached.
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Creates a <see cref="ForwardSpec"/> from its <see cref="string"/> representation.
        /// </summary>
        /// <param name="spec">A <see cref="string"/> which represents a <see cref="ForwardSpec"/>.</param>
        /// <returns>A <see cref="ForwardSpec"/> which represents <paramref name="spec"/>.</returns>
        public static ForwardSpec Parse(string spec) => new(spec);

        /// <inheritdoc/>
        public override string ToString()
        {
            string protocolString = Mappings.FirstOrDefault(v => v.Value == Protocol).Key;

            return Protocol switch
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
        public override int GetHashCode() =>
            (int)Protocol
                ^ Port
                ^ ProcessId
                ^ (SocketName == null ? 1 : SocketName.GetHashCode());

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ForwardSpec other
                && other.Protocol == Protocol
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
        }
    }
}
