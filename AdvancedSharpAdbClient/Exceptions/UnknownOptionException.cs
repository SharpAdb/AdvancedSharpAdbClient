// <copyright file="UnknownOptionException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Thrown when a command has an unknown option passed.
    /// </summary>
    [Serializable]
    public class UnknownOptionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownOptionException"/> class.
        /// </summary>
        public UnknownOptionException() : base("Unknown option.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownOptionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public UnknownOptionException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownOptionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnknownOptionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

#if HAS_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownOptionException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
#if NET8_0_OR_GREATER
        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
        public UnknownOptionException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }
#endif
    }
}
