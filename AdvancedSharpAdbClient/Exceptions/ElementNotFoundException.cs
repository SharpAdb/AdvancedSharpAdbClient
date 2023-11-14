﻿// <copyright file="ElementNotFoundException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with Element.
    /// </summary>
    [Serializable]
    public class ElementNotFoundException(string? message) : Exception(message)
    {
    }
}
