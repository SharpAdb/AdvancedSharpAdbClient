// <copyright file="FileStatisticsBase<T>.Base.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Contains information about a file on the remote device.
    /// </summary>
    /// <typeparam name="T">The type of the data for file.</typeparam>
    /// <typeparam name="TSelf">The type of the derived class.</typeparam>
    /// <param name="data">The <typeparamref name="T"/> for the data of file.</param>
    public abstract class FileStatisticsBase<T, TSelf>(in T data) : IEquatable<TSelf>, IEquatable<FileStatisticsBase<T, TSelf>>
#if NET7_0_OR_GREATER
        , IEqualityOperators<TSelf, TSelf, bool>, IEqualityOperators<TSelf, FileStatisticsBase<T, TSelf>, bool>
#endif
        where T : unmanaged, IEquatable<T>
#if NET7_0_OR_GREATER
        , IEqualityOperators<T, T, bool>
#endif
        where TSelf : FileStatisticsBase<T, TSelf>
    {
        /// <summary>
        /// The <typeparamref name="T"/> for the data of file.
        /// </summary>
        protected readonly T data = data;

        /// <summary>
        /// Gets the path of the file.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is FileStatisticsBase<T, TSelf> other && Equals(other);

        /// <inheritdoc/>
        public bool Equals([NotNullWhen(true)] TSelf? other) => other is FileStatisticsBase<T, TSelf> obj && Equals(obj);

        /// <inheritdoc/>
        public bool Equals([NotNullWhen(true)] FileStatisticsBase<T, TSelf>? other) =>
            (object)this == other ||
                (other is not null
                    && Path == other.Path
#if NET7_0_OR_GREATER
                    && data == other.data);
#else
                    && data.Equals(other.data));
#endif

        /// <summary>
        /// Tests whether two <see cref="FileStatisticsBase{T, TSelf}"/> objects are equally.
        /// </summary>
        /// <param name="left">The <typeparamref name="TSelf"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="FileStatisticsBase{T, TSelf}"/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsBase{T, TSelf}"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(TSelf? left, FileStatisticsBase<T, TSelf>? right) => EqualityComparer<FileStatisticsBase<T, TSelf>?>.Default.Equals(left, right);

        /// <summary>
        /// Tests whether two <see cref="FileStatisticsBase{T, TSelf}"/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref="FileStatisticsBase{T, TSelf}"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="FileStatisticsBase{T, TSelf}"/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsBase{T, TSelf}"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(FileStatisticsBase<T, TSelf>? left, FileStatisticsBase<T, TSelf>? right) => EqualityComparer<FileStatisticsBase<T, TSelf>?>.Default.Equals(left, right);

        /// <summary>
        /// Tests whether two <see cref="FileStatisticsBase{T, TSelf}"/> objects are different.
        /// </summary>
        /// <param name="left">The <typeparamref name="TSelf"/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="FileStatisticsBase{T, TSelf}"/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsBase{T, TSelf}"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(TSelf? left, FileStatisticsBase<T, TSelf>? right) => !(left == right);

        /// <summary>
        /// Tests whether two <see cref="FileStatisticsBase{T, TSelf}"/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref="FileStatisticsBase{T, TSelf}"/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="FileStatisticsBase{T, TSelf}"/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsBase{T, TSelf}"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(FileStatisticsBase<T, TSelf>? left, FileStatisticsBase<T, TSelf>? right) => !(left == right);

#if NET7_0_OR_GREATER
        /// <inheritdoc/>
        static bool IEqualityOperators<TSelf, TSelf, bool>.operator ==(TSelf? left, TSelf? right) => EqualityComparer<TSelf?>.Default.Equals(left, right);

        /// <inheritdoc/>
        static bool IEqualityOperators<TSelf, TSelf, bool>.operator !=(TSelf? left, TSelf? right) => !EqualityComparer<TSelf?>.Default.Equals(left, right);
#endif

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Path, data);
    }
}
