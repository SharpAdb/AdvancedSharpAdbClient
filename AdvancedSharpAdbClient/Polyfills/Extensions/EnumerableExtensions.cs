// <copyright file="EnumerableExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

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
        public static void AddRange<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            ExceptionExtensions.ThrowIfNull(source);
            ExceptionExtensions.ThrowIfNull(collection);

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

#if HAS_TASK
        /// <summary>
        /// Asynchronously creates an array from a <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/> to create an array from.</param>
        /// <returns>A <see cref="Task{Array}"/> which returns an array that contains the elements from the input sequence.</returns>
        public static Task<TSource[]> ToArrayAsync<TSource>(this Task<IEnumerable<TSource>> source) =>
            source.ContinueWith(x => x.Result.ToArray());

        /// <summary>
        /// Asynchronously creates an array from a <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/> to create an array from.</param>
        /// <returns>A <see cref="Task{Array}"/> which returns an array that contains the elements from the input sequence.</returns>
        public static Task<TSource[]> ToArrayAsync<TSource>(this IEnumerable<Task<TSource>> source) =>
            source.WhenAll();

#if COMP_NETSTANDARD2_1
        /// <summary>
        /// Asynchronously creates a <see cref="List{T}"/> from an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IAsyncEnumerable{T}"/> to create a <see cref="List{T}"/> from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{List}"/> which returns a <see cref="List{T}"/> that contains elements from the input sequence.</returns>
        public static async ValueTask<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            List<TSource> list = [];
            await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Returns the input typed as <see cref="IAsyncEnumerable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to type as <see cref="IAsyncEnumerable{TSource}"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>The input sequence typed as <see cref="IAsyncEnumerable{TSource}"/>.</returns>
        public static async IAsyncEnumerable<TSource> AsEnumerableAsync<TSource>(this IEnumerable<TSource> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using IEnumerator<TSource> enumerator = source.GetEnumerator();
            while (!cancellationToken.IsCancellationRequested && enumerator.MoveNext())
            {
                await Task.Yield();
                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// Returns the input typed as <see cref="IAsyncEnumerable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to type as <see cref="IAsyncEnumerable{TSource}"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>The input sequence typed as <see cref="IAsyncEnumerable{TSource}"/>.</returns>
        public static async IAsyncEnumerable<TSource> AsEnumerableAsync<TSource>(this Task<IEnumerable<TSource>> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using IEnumerator<TSource> enumerator = await source.ContinueWith(x => x.Result.GetEnumerator()).ConfigureAwait(false);
            while (!cancellationToken.IsCancellationRequested && enumerator.MoveNext())
            {
                await Task.Yield();
                yield return enumerator.Current;
            }
        }
#endif
#endif
    }
}
