#if NETFRAMEWORK && !NET45_OR_GREATER
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a strongly-typed, read-only collection of elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
#if NET40_OR_GREATER
    internal interface IReadOnlyCollection<out T> : IEnumerable<T>
#else
    internal interface IReadOnlyCollection<T> : IEnumerable<T>
#endif
    {
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The number of elements in the collection.</value>
        int Count { get; }
    }
}
#else
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Collections.Generic.IReadOnlyCollection<>))]
#endif