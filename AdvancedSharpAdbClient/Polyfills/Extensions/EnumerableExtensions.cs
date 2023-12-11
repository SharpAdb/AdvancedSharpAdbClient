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
    public static class EnumerableExtensions
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
        /// Asynchronously creates an array from a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create an array from.</param>
        /// <returns>An array that contains the elements from the input sequence.</returns>
        public static Task<TSource[]> ToArrayAsync<TSource>(this Task<IEnumerable<TSource>> source) =>
            source.ContinueWith(x => x.Result.ToArray());

        /// <summary>
        /// Asynchronously creates an array from a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create an array from.</param>
        /// <returns>An array that contains the elements from the input sequence.</returns>
        public static Task<TSource[]> ToArrayAsync<TSource>(this IEnumerable<Task<TSource>> source) =>
            Extensions.WhenAll(source);
#endif
    }
}
