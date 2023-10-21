// <copyright file="AdbException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with communicating with ADB.
    /// </summary>
    [Serializable]
    public class AdbException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class.
        /// </summary>
        public AdbException() : base("An error occurred with ADB")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AdbException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class with the specified client error message and adb error message.
        /// </summary>
        /// <param name="message">The message that describes the error on the client side.</param>
        /// <param name="adbError">The raw error message that was sent by adb.</param>
        public AdbException(string? message, string? adbError) : base(message) => AdbError = adbError;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class with the specified client error message and <see cref="AdbResponse"/>
        /// </summary>
        /// <param name="message">The message that describes the error on the client side.</param>
        /// <param name="response">The <see cref="AdbResponse"/> that was sent by adb.</param>
        public AdbException(string? message, AdbResponse response) : base(message)
        {
            AdbError = response.Message;
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public AdbException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

#if HAS_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
#if NET8_0_OR_GREATER
        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
        public AdbException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }
#endif

        /// <summary>
        /// Gets the error message that was sent by adb.
        /// </summary>
        public string? AdbError { get; init; }

        /// <summary>
        /// Gets the response that was sent by adb.
        /// </summary>
        public AdbResponse Response { get; init; }

        /// <summary>
        /// Gets a value indicating whether the underlying error was a <see cref="SocketException"/> where the
        /// <see cref="SocketException.SocketErrorCode"/> is set to <see cref="SocketError.ConnectionReset"/>, indicating
        /// that the connection was reset by the remote server. This happens when the adb server was killed.
        /// </summary>
        public bool ConnectionReset =>
            InnerException is SocketException socketException
            && socketException.SocketErrorCode == SocketError.ConnectionReset;
    }
}
