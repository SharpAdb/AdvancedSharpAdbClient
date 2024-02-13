#if NETFRAMEWORK && !NET40_OR_GREATER
// <copyright file="StringBuilderExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Text;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="StringBuilder"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Removes all characters from the current <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to removes all characters.</param>
        /// <returns>An object whose <see cref="StringBuilder.Length"/> is 0 (zero).</returns>
        public static StringBuilder Clear(this StringBuilder builder)
        {
            builder.Length = 0;
            return builder;
        }
    }
}
#endif