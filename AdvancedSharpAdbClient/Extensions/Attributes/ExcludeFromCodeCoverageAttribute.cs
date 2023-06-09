#if NET35
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies that the attributed code should be excluded from code coverage
    /// collection.  Placing this attribute on a class/struct excludes all
    /// enclosed methods and properties from code coverage collection.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event,
        Inherited = false,
        AllowMultiple = false
        )]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        public ExcludeFromCodeCoverageAttribute() { }
    }
}
#endif