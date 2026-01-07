// <copyright file="NullLoggerT.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Represents a type used to configure the logging system and create instances of <see cref="ILogger"/>.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        ILogger CreateLogger(string categoryName);

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <typeparam name="T">The category name for messages produced by the logger.</typeparam>
        /// <returns>The <see cref="ILogger"/>.</returns>
        ILogger<T> CreateLogger<T>();
    }
}
