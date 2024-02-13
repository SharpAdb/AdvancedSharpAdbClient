#if NETFRAMEWORK && !NET40_OR_GREATER
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace System.Net
{
    /// <summary>
    /// Represents a network endpoint as a host name or a string representation of an IP address and a port number.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DnsEndPoint : EndPoint
    {
        private readonly AddressFamily _family;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsEndPoint"/> class with the host name or string representation of an IP address and a port number.
        /// </summary>
        /// <param name="host">The host name or a string representation of the IP address.</param>
        /// <param name="port">The port number associated with the address, or 0 to specify any available port. <paramref name="port"/> is in host order.</param>
        public DnsEndPoint(string host, int port) : this(host, port, AddressFamily.Unspecified) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsEndPoint"/> class with the host name or string representation of an IP address, a port number, and an address family.
        /// </summary>
        /// <param name="host">The host name or a string representation of the IP address.</param>
        /// <param name="port">The port number associated with the address, or 0 to specify any available port. port is in host order.</param>
        /// <param name="addressFamily">One of the <see cref="AddressFamily"/> values.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is less than <see cref="IPEndPoint.MinPort"/>.<para>-or-</para><paramref name="port"/> is greater than <see cref="IPEndPoint.MaxPort"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="host"/> parameter contains an empty string.<para>-or-</para><paramref name="addressFamily"/> is Unknown.</exception>
        public DnsEndPoint(string host, int port, AddressFamily addressFamily)
        {
            ExceptionExtensions.ThrowIfNull(host);

            if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (addressFamily is not AddressFamily.InterNetwork and
                not AddressFamily.InterNetworkV6 and
                not AddressFamily.Unspecified)
            {
                throw new ArgumentException("The specified value is not valid.", nameof(addressFamily));
            }

            Host = host;
            Port = port;
            _family = addressFamily;
        }

        /// <summary>
        /// Compares two <see cref="DnsEndPoint"/> objects.
        /// </summary>
        /// <param name="comparand">A <see cref="DnsEndPoint"/> instance to compare to the current instance.</param>
        /// <returns><see langword="true"/> if the two <see cref="DnsEndPoint"/> instances are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals([NotNullWhen(true)] object? comparand)
        {
            return comparand is DnsEndPoint dnsComparand
                && _family == dnsComparand._family &&
                    Port == dnsComparand.Port &&
                    Host == dnsComparand.Host;
        }

        /// <summary>
        /// Returns a hash value for a <see cref="DnsEndPoint"/>.
        /// </summary>
        /// <returns>An integer hash value for the <see cref="DnsEndPoint"/>.</returns>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());
        }

        /// <summary>
        /// Returns the host name or string representation of the IP address and port number of the <see cref="DnsEndPoint"/>.
        /// </summary>
        /// <returns>A string containing the address family, host name or IP address string, and the port number of the specified <see cref="DnsEndPoint"/>.</returns>
        public override string ToString() => _family + "/" + Host + ":" + Port;

        /// <summary>
        /// Gets the host name or string representation of the Internet Protocol (IP) address of the host.
        /// </summary>
        /// <value>A host name or string representation of an IP address.</value>
        public string Host { get; }

        /// <summary>
        /// Gets the Internet Protocol (IP) address family.
        /// </summary>
        /// <value>One of the <see cref="AddressFamily"/> values.</value>
        public override AddressFamily AddressFamily => _family;

        /// <summary>
        /// Gets the port number of the <see cref="DnsEndPoint"/>.
        /// </summary>
        /// <value>An integer value in the range 0 to 0xffff indicating the port number of the <see cref="DnsEndPoint"/>.</value>
        public int Port { get; }
    }
}
#else
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Net.EndPoint))]
#endif