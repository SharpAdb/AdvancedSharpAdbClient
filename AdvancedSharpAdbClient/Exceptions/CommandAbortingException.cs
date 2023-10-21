// <copyright file="CommandAbortingException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Thrown when an executed command identifies that it is being aborted.
    /// </summary>
    [Serializable]
    public class CommandAbortingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAbortingException"/> class.
        /// </summary>
        public CommandAbortingException() : base("Permission to access the specified resource was denied.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAbortingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CommandAbortingException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAbortingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandAbortingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

#if HAS_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAbortingException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
#if NET8_0_OR_GREATER
        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
        public CommandAbortingException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }
#endif
    }
}
