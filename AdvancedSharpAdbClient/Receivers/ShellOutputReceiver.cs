// <copyright file="ShellOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Receivers
{
    /// <summary>
    /// A multiline receiver to receive and process shell output with multi lines.
    /// </summary>
    public abstract partial class ShellOutputReceiver : IShellOutputReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellOutputReceiver"/> class.
        /// </summary>
        public ShellOutputReceiver() { }

        /// <inheritdoc/>
        public abstract bool AddOutput(string line);

        /// <inheritdoc/>
        public virtual void Flush() => Done();

        /// <summary>
        /// Finishes the receiver.
        /// </summary>
        protected virtual void Done()
        {
            // Do nothing
        }
    }
}
