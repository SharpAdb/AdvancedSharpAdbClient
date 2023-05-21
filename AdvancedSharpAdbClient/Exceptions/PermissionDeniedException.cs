// <copyright file="PermissionDeniedException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

#if HAS_Serialization
using System.Runtime.Serialization;
#endif

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the permission to a resource was denied.
    /// </summary>
    public class PermissionDeniedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        public PermissionDeniedException() : base("Permission to access the specified resource was denied.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PermissionDeniedException(string message) : base(message)
        {
        }

#if HAS_Serialization
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        public PermissionDeniedException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PermissionDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
