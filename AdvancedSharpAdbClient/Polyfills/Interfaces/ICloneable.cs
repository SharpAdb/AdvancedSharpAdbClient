// <copyright file="ICloneable.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

#if !NETFRAMEWORK && !NETCOREAPP2_0_OR_GREATER && !NETSTANDARD2_0_OR_GREATER && !UAP10_0_15138_0
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System
{
    /// <summary>
    /// Supports cloning, which creates a new instance of a class with the same value as an existing instance.
    /// </summary>
    internal interface ICloneable
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        object Clone();
    }
}

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Supports cloning, which creates a new instance of a class with the same value as an existing instance.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    public interface ICloneable<out T>
    {
        /// <summary>
        /// Creates a new <typeparamref name="T"/> object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new <typeparamref name="T"/> object that is a copy of this instance.</returns>
        T Clone();
    }
}
#else
namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Supports cloning, which creates a new instance of a class with the same value as an existing instance.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    public interface ICloneable<out T> : ICloneable
    {
        /// <summary>
        /// Creates a new <typeparamref name="T"/> object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new <typeparamref name="T"/> object that is a copy of this instance.</returns>
        new T Clone();
    }
}
#endif