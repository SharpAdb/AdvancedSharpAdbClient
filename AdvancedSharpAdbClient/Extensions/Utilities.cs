// <copyright file="Utilities.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if NETSTANDARD1_3
using System.Net.Sockets;
#endif

namespace AdvancedSharpAdbClient
{
    internal static class Utilities
    {
        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct =>
#if NET35
            EnumEx
#else
            Enum
#endif
            .TryParse(value, ignoreCase, out result);

        public static bool IsNullOrWhiteSpace(this string value) =>
#if NET35
            StringEx
#else
            string
#endif
            .IsNullOrWhiteSpace(value);

        public static string Join(string separator, IEnumerable<string> values) =>
#if NET35
            StringEx
#else
            string
#endif
            .Join(separator, values);

        public static Task Delay(int dueTime) =>
#if NET35 || NET40
            TaskEx
#else
            Task
#endif
            .Delay(dueTime);

        public static Task Run(Func<Task> function) =>
#if NET35 || NET40
            TaskEx
#else
            Task
#endif
            .Run(function);

        public static Task<TResult> Run<TResult>(Func<TResult> function) =>
#if NET35 || NET40
            TaskEx
#else
            Task
#endif
            .Run(function);

        public static DateTimeOffset FromUnixTimeSeconds(long seconds) =>
#if NET35 || NET40 || NET452
            new(seconds.ToDateTime());
#else
            DateTimeOffset.FromUnixTimeSeconds(seconds);
#endif

        public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset) =>
#if NET35 || NET40 || NET452
            (int)dateTimeOffset.DateTime.ToUnixEpoch();
#else
            (int)dateTimeOffset.ToUnixTimeSeconds();
#endif

#if NETSTANDARD1_3
        public static IAsyncResult BeginReceive(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state) =>
            TaskToApm.Begin(socket.ReceiveAsync(buffer, offset, size, socketFlags, default), callback, state);
#endif

#if NETSTANDARD1_3
        public static int EndReceive(this Socket _, IAsyncResult asyncResult) =>
            TaskToApm.End<int>(asyncResult);
#endif
    }
}
