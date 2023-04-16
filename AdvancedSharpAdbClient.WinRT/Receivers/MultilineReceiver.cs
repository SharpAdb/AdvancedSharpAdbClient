// <copyright file="MultiLineReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Multi Line Receiver
    /// </summary>
    public interface IMultiLineReceiver : IShellOutputReceiver
    {
        /// <summary>
        /// Gets or sets a value indicating whether [trim lines].
        /// </summary>
        /// <value><see langword="true"/> if [trim lines]; otherwise, <see langword="false"/>.</value>
        bool TrimLines { get; set; }
    }
}
