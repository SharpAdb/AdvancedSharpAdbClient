// <copyright file="AdbResponse.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// The response returned by ADB server.
    /// </summary>
    /// <param name="message">the message of <see cref="AdbResponse"/>.</param>
    [DebuggerDisplay($"{nameof(AdbResponse)} \\{{ {nameof(IOSuccess)} = {{{nameof(IOSuccess)}}}, {nameof(Okay)} = {{{nameof(Okay)}}}, {nameof(Timeout)} = {{{nameof(Timeout)}}}, {nameof(Message)} = {{{nameof(Message)}}} }}")]
    public readonly struct AdbResponse(string message) : IEquatable<AdbResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbResponse"/> struct.
        /// </summary>
        public AdbResponse() : this(string.Empty) { }

        /// <summary>
        /// Gets a <see cref="AdbResponse"/> that represents the OK response sent by ADB.
        /// </summary>
        public static AdbResponse OK { get; } = new()
        {
            IOSuccess = true,
            Okay = true,
            Timeout = false
        };

        /// <summary>
        /// Gets or sets a value indicating whether the IO communication was a success.
        /// </summary>
        /// <value><see langword="true"/> if successful; otherwise, <see langword="false"/>.</value>
        public bool IOSuccess { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is okay.
        /// </summary>
        /// <value><see langword="true"/> if okay; otherwise, <see langword="false"/>.</value>
        public bool Okay { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is timeout.
        /// </summary>
        /// <value><see langword="true"/> if timeout; otherwise, <see langword="false"/>.</value>
        public bool Timeout { get; init; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; init; } = message;

        /// <summary>
        /// Throw <see cref="AdbException"/> if <see cref="IOSuccess"/> or <see cref="Okay"/> is <see langword="false"/>.
        /// </summary>
        public void Throw()
        {
            if (!IOSuccess || !Okay)
            {
                throw new AdbException($"An error occurred while reading a response from ADB: {Message}", this);
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AdbResponse"/> class, based on an
        /// error message returned by adb.
        /// </summary>
        /// <param name="message">The error message returned by adb.</param>
        /// <returns>A new <see cref="AdbResponse"/> object that represents the error.</returns>
        public static AdbResponse FromError(string message) => new(message)
        {
            IOSuccess = true,
            Okay = false,
            Timeout = false
        };

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is AdbResponse other && Equals(other);

        /// <inheritdoc/>
        public bool Equals(AdbResponse other) =>
            IOSuccess == other.IOSuccess
                && string.Equals(Message, other.Message, StringComparison.OrdinalIgnoreCase)
                && Okay == other.Okay
                && Timeout == other.Timeout;

        /// <summary>
        /// Tests whether two <see cref='AdbResponse'/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref='AdbResponse'/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref='AdbResponse'/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="AdbResponse"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(AdbResponse left, AdbResponse right) => left.Equals(right);

        /// <summary>
        /// Tests whether two <see cref='AdbResponse'/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref='AdbResponse'/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref='AdbResponse'/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="AdbResponse"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(AdbResponse left, AdbResponse right) => !left.Equals(right);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(IOSuccess, Message, Okay, Timeout);

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="AdbResponse"/>.
        /// </summary>
        /// <returns><c>OK</c> if the response is an OK response, or <c>Error: {Message}</c> if the response indicates an error.</returns>
        public override string ToString() => Equals(OK) ? "OK" : $"Error: {Message}";
    }
}
