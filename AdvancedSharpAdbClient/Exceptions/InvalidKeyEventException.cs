// <copyright file="InvalidKeyEventException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with key event.
    /// </summary>
    [Serializable]
    public class InvalidKeyEventException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidKeyEventException"/> class.
        /// </summary>
        public InvalidKeyEventException(string message) : base(message)
        {
        }
    }
}
