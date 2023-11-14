// <copyright file="NullLogger.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Minimalistic logger that does nothing.
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <summary>
        /// Returns the shared instance of <see cref="NullLogger"/>.
        /// </summary>
        public static NullLogger Instance { get; } = new();

        /// <inheritdoc />
        public void Log(LogLevel logLevel, Exception? exception, string? message, params object?[] args) { }
    }

    /// <summary>
    /// Minimalistic logger that does nothing.
    /// </summary>
    public class NullLogger<T> : NullLogger, ILogger<T>
    {
        /// <summary>
        /// Returns an instance of <see cref="NullLogger{T}"/>.
        /// </summary>
        /// <returns>An instance of <see cref="NullLogger{T}"/>.</returns>
        public static new NullLogger<T> Instance { get; } = new();
    }
}
