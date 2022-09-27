﻿// <copyright file="DeviceNotFoundException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Unable to connect to the device because it was not found in the list of available devices.
    /// </summary>
    public class DeviceNotFoundException : AdbException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNotFoundException"/> class.
        /// </summary>
        public DeviceNotFoundException() : base("The device was not found.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNotFoundException"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public DeviceNotFoundException(string device) : base("The device '" + device + "' was not found.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DeviceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNotFoundException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        public DeviceNotFoundException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }
    }
}
