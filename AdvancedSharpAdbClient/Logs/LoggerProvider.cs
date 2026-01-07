// <copyright file="LoggerProvider.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Provides a mechanism for creating instances of <see cref="ILogger"/> and <see cref="ILogger{T}"/> classes.
    /// </summary>
    public static class LoggerProvider
    {
        /// <summary>
        /// The current log provider.
        /// </summary>
        private static ILoggerFactory? _loggerFactory = null;

        /// <summary>
        /// Sets the current log provider based on logger factory.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public static void SetLogProvider(ILoggerFactory? loggerFactory) => _loggerFactory = loggerFactory;

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <param name="category">The category name for messages produced by the logger.</param>
        /// <returns>A new <see cref="ILogger"/> instance.</returns>
        public static ILogger CreateLogger(string category) => _loggerFactory == null ? NullLogger.Instance : _loggerFactory.CreateLogger(category);

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance using the full name of the given type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The <see cref="ILogger"/> that was created</returns>
        public static ILogger<T> CreateLogger<T>() => _loggerFactory == null ? NullLogger<T>.Instance : _loggerFactory.CreateLogger<T>();
    }
}
