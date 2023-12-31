// <copyright file="FunctionOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Receivers
{
    /// <summary>
    /// Provides a <see cref="IShellOutputReceiver"/> which calls a function for each line of output
    /// </summary>
    /// <param name="predicate">The function to call for each line of output</param>
    public readonly partial struct FunctionOutputReceiver(Func<string, bool> predicate) : IShellOutputReceiver
    {
        /// <inheritdoc/>
        public bool AddOutput(string line) => predicate(line);

        /// <inheritdoc/>
        public void Flush()
        {
            // Do nothing
        }
    }
}
