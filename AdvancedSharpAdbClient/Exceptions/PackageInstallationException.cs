// <copyright file="PackageInstallationException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

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
        public PackageInstallationException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public PackageInstallationException(string? message, Exception? inner) : base(message, inner)
        {
        }

#if HAS_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
        /// <exception cref="SerializationException">The class name is null or <see cref="Exception.HResult"/> is zero (0).</exception>
#if NET8_0_OR_GREATER
        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
        protected PackageInstallationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
