// <copyright file="EnumerableExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="Enumerable"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ICollection{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="ICollection{TSource}"/> to be added.</param>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ICollection{TSource}"/>.
        /// The collection itself cannot be <see langword="null"/>, but it can contain elements that are
        /// <see langword="null"/>, if type <typeparamref name="TSource"/> is a reference type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="collection"/> is null.</exception>
        public static void AddRange<TSource>(this ICollection<TSource> source, params IEnumerable<TSource> collection)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(collection);

            if (source is List<TSource> list)
            {
                list.AddRange(collection);
            }
#if !NETFRAMEWORK || NET40_OR_GREATER
            else if (source is ISet<TSource> set)
            {
                set.UnionWith(collection);
            }
#endif
            else
            {
                foreach (TSource item in collection)
                {
                    source.Add(item);
                }
            }
        }

#if COMP_NETSTANDARD2_1
        /// <summary>
        /// Creates a new <see cref="IAsyncEnumerable{T}"/> that iterates through <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="Task{T}"/> of the <see cref="IEnumerable{T}"/> of the elements to enumerate.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing the sequence of elements from <paramref name="source"/>.</returns>
        public static async IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this Task<IEnumerable<TSource>> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            IEnumerable<TSource> enumerable = await source.ConfigureAwait(false);
            switch (enumerable)
            {
                case null:
                    throw new NullReferenceException("The source task completed to a null enumerable.");
                case TSource[] array:
                    foreach (TSource item in array)
                    {
                        yield return item;
                    }
                    yield break;
                default:
                    foreach (TSource item in enumerable)
                    {
                        yield return item;
                    }
                    yield break;
            }
        }
#endif
    }
}
