using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

#if NET8_0_OR_GREATER
using System.Numerics;
#endif

namespace AdvancedSharpAdbClient.Exceptions
{
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.
        /// </summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull(object argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument, paramName);
#else
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
#endif
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.
        /// </summary>
        /// <param name="value">The argument to validate as less or equal than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string paramName = null)
            where T : IComparable<T>
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName);
#else
            if (value.CompareTo(other) > 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be less than or equal to '{value}'.");
            }
#endif
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
        /// </summary>
        /// <param name="value">The argument to validate as greater than or equal than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string paramName = null)
            where T : IComparable<T>
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName);
#else
            if (value.CompareTo(other) < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be greater than or equal to '{value}'.");
            }
#endif
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
        /// </summary>
        /// <param name="value">The argument to validate as non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET8_0_OR_GREATER
        public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string paramName = null)
            where T : INumberBase<T>
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);
#else
        public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be a non-negative value.");
            }
#endif
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="instance">The object whose type's full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
        /// <exception cref="ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
#if NET6_0_OR_GREATER
        [StackTraceHidden]
#endif
        public static void ThrowIf(
#if HAS_IndexRange
            [DoesNotReturnIf(true)]
#endif
            bool condition, object instance)
        {
#if NET7_0_OR_GREATER
            ObjectDisposedException.ThrowIf(condition, instance);
#else
            if (condition)
            {
                throw new ObjectDisposedException(instance?.GetType().FullName);
            }
#endif
        }
    }
}
