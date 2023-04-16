// <copyright file="CommandAbortingException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

#if HAS_Serialization
using System.Runtime.Serialization;
#endif

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Thrown when an executed command identifies that it is being aborted.
    /// </summary>
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
        public CommandAbortingException(string message) : base(message)
        {
        }

#if HAS_Serialization
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAbortingException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        public CommandAbortingException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAbortingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandAbortingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
