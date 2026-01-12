#if COMP_NETSTANDARD2_1 && !NET10_0_OR_GREATER
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;

namespace System.Linq
{
    /// <summary>
    /// Provides a set of static methods for querying objects that implement <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    internal static class AsyncEnumerable
    {
        /// <summary>
        /// Creates a new <see cref="IAsyncEnumerable{T}"/> that iterates through <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of the elements to enumerate.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> containing the sequence of elements from <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>Each iteration through the resulting <see cref="IAsyncEnumerable{T}"/> will iterate through the <paramref name="source"/>.</remarks>
        public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source switch
            {
                TSource[] array => array.Length == 0 ? Empty<TSource>() : FromArray(array),
                List<TSource> list => FromList(list),
                IList<TSource> list => FromIList(list),
                _ when source == Enumerable.Empty<TSource>() => Empty<TSource>(),
                _ => FromIterator(source),
            };

            static async IAsyncEnumerable<TSource> FromArray(TSource[] source)
            {
                for (int i = 0; ; i++)
                {
                    int localI = i;
                    TSource[] localSource = source;
                    if ((uint)localI >= (uint)localSource.Length)
                    {
                        break;
                    }
                    yield return localSource[localI];
                }
            }

            static async IAsyncEnumerable<TSource> FromList(List<TSource> source)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    yield return source[i];
                }
            }

            static async IAsyncEnumerable<TSource> FromIList(IList<TSource> source)
            {
                int count = source.Count;
                for (int i = 0; i < count; i++)
                {
                    yield return source[i];
                }
            }

            static async IAsyncEnumerable<TSource> FromIterator(IEnumerable<TSource> source)
            {
                foreach (TSource element in source)
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Returns an empty <see cref="IAsyncEnumerable{T}"/> that has the specified type argument.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements of the sequence.</typeparam>
        /// <returns>An empty <see cref="IAsyncEnumerable{T}"/> whose type argument is <typeparamref name="TResult"/>.</returns>
        public static IAsyncEnumerable<TResult> Empty<TResult>() => EmptyAsyncEnumerable<TResult>.Instance;

        private sealed class EmptyAsyncEnumerable<TResult> : IAsyncEnumerable<TResult>, IAsyncEnumerator<TResult>
        {
            public static readonly EmptyAsyncEnumerable<TResult> Instance = new();

            public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

            public ValueTask<bool> MoveNextAsync() => default;

            public TResult Current => default!;

            public ValueTask DisposeAsync() => default;
        }
    }
}
#endif