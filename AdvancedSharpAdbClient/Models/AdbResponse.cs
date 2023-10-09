// <copyright file="AdbResponse.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// The response returned by ADB server.
    /// </summary>
    public struct AdbResponse : IEquatable<AdbResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbResponse"/> struct.
        /// </summary>
        public AdbResponse() => Message = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbResponse"/> struct.
        /// </summary>
        /// <param name="message">the message of <see cref="AdbResponse"/>.</param>
        public AdbResponse(string message) => Message = message;

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
        public bool IOSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is okay.
        /// </summary>
        /// <value><see langword="true"/> if okay; otherwise, <see langword="false"/>.</value>
        public bool Okay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is timeout.
        /// </summary>
        /// <value><see langword="true"/> if timeout; otherwise, <see langword="false"/>.</value>
        public bool Timeout { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

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

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="AdbResponse"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="AdbResponse"/> object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj) => obj is AdbResponse other && Equals(other);

        /// <summary>
        /// Determines whether the specified <see cref="AdbResponse"/> is equal to the current <see cref="AdbResponse"/> object.
        /// </summary>
        /// <param name="other">The <see cref="AdbResponse"/> to compare with the current <see cref="AdbResponse"/> object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(AdbResponse other) =>
            other.IOSuccess == IOSuccess
            && string.Equals(other.Message, Message, StringComparison.OrdinalIgnoreCase)
            && other.Okay == Okay
            && other.Timeout == Timeout;

        /// <summary>
        /// Gets the hash code for the current <see cref="AdbResponse"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="AdbResponse"/>.</returns>
        public override readonly int GetHashCode()
        {
            int hash = 17;
            hash = (hash * 23) + IOSuccess.GetHashCode();
            hash = (hash * 23) + Message == null ? 0 : Message.GetHashCode();
            hash = (hash * 23) + Okay.GetHashCode();
            hash = (hash * 23) + Timeout.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="AdbResponse"/>.
        /// </summary>
        /// <returns><c>OK</c> if the response is an OK response, or <c>Error: {Message}</c> if the response indicates an error.</returns>
        public override readonly string ToString() => Equals(OK) ? "OK" : $"Error: {Message}";

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
    }
}
