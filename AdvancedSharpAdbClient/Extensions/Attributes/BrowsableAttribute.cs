#if NETSTANDARD && !NETSTANDARD2_0_OR_GREATER || NETCORE && !UAP10_0_15138_0
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel
{
    /// <summary>
    /// Specifies whether a property or event should be displayed in a property
    /// browsing window.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class BrowsableAttribute : Attribute
    {
        /// <summary>
        /// Specifies that a property or event can be modified at design time.
        /// This <see langword='static '/> field is read-only.
        /// </summary>
        public static readonly BrowsableAttribute Yes = new(true);

        /// <summary>
        /// Specifies that a property or event cannot be modified at design time.'
        /// This <see langword='static'/> field is read-only.
        /// </summary>
        public static readonly BrowsableAttribute No = new(false);

        /// <summary>
        /// Specifies the default value for the <see cref='BrowsableAttribute'/>, which is <see cref='Yes'/>.
        /// This <see langword='static'/> field is read-only.
        /// </summary>
        public static readonly BrowsableAttribute Default = Yes;

        /// <summary>
        /// Initializes a new instance of the <see cref='BrowsableAttribute'/> class.
        /// </summary>
        public BrowsableAttribute(bool browsable)
        {
            Browsable = browsable;
        }

        /// <summary>
        /// Gets a value indicating whether an object is browsable.
        /// </summary>
        public bool Browsable { get; }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) =>
            obj is BrowsableAttribute other && other.Browsable == Browsable;

        /// <inheritdoc/>
        public override int GetHashCode() => Browsable.GetHashCode();

        /// <summary>
        /// Determines if this attribute is the default.
        /// </summary>
        /// <returns><see langword="true"/> if the attribute is the default value for this attribute class; otherwise, <see langword="false"/>.</returns>
        public bool IsDefaultAttribute() => Equals(Default);
    }
}
#endif