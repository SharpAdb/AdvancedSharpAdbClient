#if NETCOREAPP3_0_OR_GREATER
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Runtime.CompilerServices.Unsafe))]
#else
// <copyright file="Unsafe.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Contains generic, low-level functionality for manipulating managed and unmanaged pointers.
    /// </summary>
    public static class Unsafe
    {
        /// <summary>
        /// Reinterprets the given managed pointer as a new managed pointer to a value of type <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of managed pointer to reinterpret.</typeparam>
        /// <typeparam name="TTo">The desired type of the managed pointer.</typeparam>
        /// <param name="source">The managed pointer to reinterpret.</param>
        /// <returns>A managed pointer to a value of type <typeparamref name="TTo"/>.</returns>
        [MethodImpl((MethodImplOptions)0x100)]
        public static unsafe ref TTo As<TFrom, TTo>(ref TFrom source)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            fixed (TFrom* ptr = &source)
            {
                return ref *(TTo*)ptr;
            }
        }

#if HAS_BUFFERS
        /// <summary>
        /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The elemental type of the managed pointer.</typeparam>
        /// <param name="source">The read-only reference to reinterpret.</param>
        /// <returns>A mutable reference to a value of type <typeparamref name="T"/>.</returns>
        /// <remarks>The lifetime of the reference will not be validated when using this API.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T AsRef<T>(scoped ref readonly T source) where T : unmanaged
        {
            fixed (T* ptr = &source)
            {
                return ref *ptr;
            }
        }
#endif
    }
}
#endif