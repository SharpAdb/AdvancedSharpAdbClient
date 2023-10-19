// <copyright file="ElementNotFoundException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with Element.
    /// </summary>
    [Serializable]
    public class ElementNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementNotFoundException"/> class.
        /// </summary>
        public ElementNotFoundException(string message) : base(message)
        {
        }
    }
}
