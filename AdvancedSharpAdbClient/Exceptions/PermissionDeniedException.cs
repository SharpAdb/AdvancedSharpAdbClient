// <copyright file="PermissionDeniedException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Runtime.Serialization;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        public PermissionDeniedException(SerializationInfo serializationInfo, StreamingContext context) :
#if !NETSTANDARD1_3
            base(serializationInfo, context)
#else
            base(serializationInfo.GetString("Message"))
#endif
        {
#if NETSTANDARD1_3
            HelpLink = serializationInfo.GetString("HelpURL"); // Do not rename (binary serialization)
            HResult = serializationInfo.GetInt32("HResult"); // Do not rename (binary serialization)
            Source = serializationInfo.GetString("Source"); // Do not rename (binary serialization)
#endif
        }

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
