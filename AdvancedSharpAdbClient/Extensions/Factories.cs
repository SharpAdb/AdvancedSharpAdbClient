﻿// <copyright file="Factories.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    using System;
    using System.Net;

    /// <summary>
    /// Provides factory methods used by the various SharpAdbClient classes.
    /// </summary>
    public static class Factories
    {
        static Factories()
        {
            Reset();
        }

        /// <summary>
        /// Gets or sets a delegate which creates a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AdbSocket"/> class.
        /// </returns>
        public static Func<EndPoint, IAdbSocket> AdbSocketFactory
        { get; set; }

        /// <summary>
        /// Gets or sets a delegate that creates a new instance of the <see cref="AdvancedAdbClient"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AdvancedAdbClient"/> class.
        /// </returns>
        public static Func<EndPoint, IAdvancedAdbClient> AdbClientFactory
        { get; set; }

        /// <summary>
        /// Gets or sets a function that returns a new instance of a class that implements the
        /// <see cref="IAdbCommandLineClient"/> interface, that can be used to interact with the
        /// <c>adb.exe</c> command line client.
        /// </summary>
        public static Func<string, IAdbCommandLineClient> AdbCommandLineClientFactory
        { get; set; }

        /// <summary>
        /// Gets or sets a function that returns a new instance of a class that implements the
        /// <see cref="ISyncService"/> interface, that can be used to transfer files to and from
        /// a given device.
        /// </summary>
        public static Func<IAdvancedAdbClient, DeviceData, ISyncService> SyncServiceFactory
        { get; set; }

        /// <summary>
        /// Resets all factories to their default values.
        /// </summary>
        public static void Reset()
        {
            AdbSocketFactory = (endPoint) => new AdbSocket(endPoint);
            AdbClientFactory = (endPoint) => new AdvancedAdbClient(endPoint, Factories.AdbSocketFactory);
            AdbCommandLineClientFactory = (path) => new AdbCommandLineClient(path);
            SyncServiceFactory = (client, device) => new SyncService(client, device);
        }
    }
}
