#if (NETSTANDARD && !NETSTANDARD2_0_OR_GREATER) || (NETCORE && !UAP10_0_15138_0)
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Indicates that a class can be serialized using binary or XML serialization. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class SerializableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableAttribute"/> class.
        /// </summary>
        public SerializableAttribute() { }
    }
}
#endif