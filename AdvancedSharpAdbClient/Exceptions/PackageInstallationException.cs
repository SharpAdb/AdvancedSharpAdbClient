// <copyright file="PackageInstallationException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// An exception while installing a package on the device.
    /// </summary>
    [Serializable]
    public class PackageInstallationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        public PackageInstallationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PackageInstallationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public PackageInstallationException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
        /// <exception cref="SerializationException">The class name is null or <see cref="Exception.HResult"/> is zero (0).</exception>
        protected PackageInstallationException(SerializationInfo info, StreamingContext context) :
#if !NETSTANDARD1_3
            base(info, context)
#else
            base(info.GetString("Message"))
#endif
        {
#if NETSTANDARD1_3
            HelpLink = info.GetString("HelpURL"); // Do not rename (binary serialization)
            HResult = info.GetInt32("HResult"); // Do not rename (binary serialization)
            Source = info.GetString("Source"); // Do not rename (binary serialization)
#endif
        }
    }
}
