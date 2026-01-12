// <copyright file="EnumerableExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Provides extension methods for the <see cref="Enumerable"/> class.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Asynchronously creates an list from a <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/> to create an list from.</param>
        /// <returns>A <see cref="Task{Array}"/> which returns an list that contains the elements from the input sequence.</returns>
        public static Task<List<TSource>> ToListAsync<TSource>(this Task<IEnumerable<TSource>> source) =>
            source.ContinueWith(x => x.Result.ToList());

        /// <summary>
        /// Asynchronously creates an array from a <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="Task{TSource}"/> to create an array from.</param>
        /// <returns>A <see cref="Task{Array}"/> which returns an array that contains the elements from the input sequence.</returns>
        public static Task<TSource[]> ToArrayAsync<TSource>(this IEnumerable<Task<TSource>> source) =>
            source.WhenAll();
    }
}
