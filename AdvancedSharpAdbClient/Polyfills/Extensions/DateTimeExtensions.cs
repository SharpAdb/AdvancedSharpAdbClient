// <copyright file="DateTimeExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides helper methods for working with Unix-based date formats.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class DateTimeExtensions
    {
#if NETFRAMEWORK && !NET46_OR_GREATER
        // Number of 100ns ticks per time unit
        internal const int MicrosecondsPerMillisecond = 1000;
        private const long TicksPerMicrosecond = 10;
        private const long TicksPerMillisecond = TicksPerMicrosecond * MicrosecondsPerMillisecond;

        private const int HoursPerDay = 24;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * HoursPerDay;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = (DaysPerYear * 4) + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = (DaysPer4Years * 25) - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = (DaysPer100Years * 4) + 1; // 146097

        // Number of days from 1/1/0001 to 12/31/1969
        internal const int DaysTo1970 = (DaysPer400Years * 4) + (DaysPer100Years * 3) + (DaysPer4Years * 17) + DaysPerYear; // 719,162
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = (DaysPer400Years * 25) - 366;  // 3652059

        internal const long MinTicks = 0;
        internal const long MaxTicks = (DaysTo10000 * TicksPerDay) - 1;

        internal const long UnixEpochTicks = DaysTo1970 * TicksPerDay;

        private const long UnixEpochSeconds = UnixEpochTicks / TimeSpan.TicksPerSecond; // 62,135,596,800

        internal const long UnixMinSeconds = (MinTicks / TimeSpan.TicksPerSecond) - UnixEpochSeconds;
        internal const long UnixMaxSeconds = (MaxTicks / TimeSpan.TicksPerSecond) - UnixEpochSeconds;
#endif

        /// <summary>
        /// Gets EPOCH time.
        /// </summary>
        public static DateTime Epoch { get; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

#if NET20
#pragma warning disable CS1574
#endif
        /// <summary>
        /// Converts a Unix time expressed as the number of seconds that have elapsed
        /// since 1970-01-01T00:00:00Z to a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="seconds">A Unix time, expressed as the number of seconds that have elapsed
        /// since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC). For Unix times before this date,
        /// its value is negative.</param>
        /// <returns>A date and time value that represents the same moment in time as the Unix time.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seconds"/> is less than -62,135,596,800.
        /// <para>-or-</para><paramref name="seconds"/> is greater than 253,402,300,799.</exception>
        /// <remarks>The Offset property value of the returned <see cref="DateTimeOffset"/> instance is
        /// <see cref="TimeSpan.Zero"/>, which represents Coordinated Universal Time. You can convert it to the time in
        /// a specific time zone by calling the <see cref="TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZoneInfo)"/> method.</remarks>
        public static DateTimeOffset FromUnixTimeSeconds(long seconds)
        {
#if NETFRAMEWORK && !NET46_OR_GREATER
            if (seconds is < UnixMinSeconds or > UnixMaxSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds),
                    $"Valid values are between {UnixMinSeconds} and {UnixMaxSeconds}, inclusive.");
            }

            long ticks = (seconds * TimeSpan.TicksPerSecond) + UnixEpochTicks;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
#else
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
#endif
        }
#if NET20
#pragma warning restore CS1574
#endif

#if NETFRAMEWORK && !NET46_OR_GREATER
        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="dateTimeOffset">The DateTimeOffset</param>
        /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
        public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset)
        {
            long seconds = dateTimeOffset.UtcDateTime.Ticks / TimeSpan.TicksPerSecond;
            return seconds - UnixEpochSeconds;
        }
#endif
    }
}
