// <copyright file="NamespaceDoc.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.ComponentModel;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers
{
    /// <summary>
    /// The classes in this namespace provide receivers for <see cref="DeviceCommands"/>.
    /// </summary>
    /// <remarks><c>Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.</c></remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal abstract class NamespaceDoc : AdvancedSharpAdbClient.Receivers.NamespaceDoc
    {
        /// <summary>
        /// The name of the namespace <see cref="Receivers"/>.
        /// </summary>
        public new const string Name = $"{DeviceCommands.NamespaceDoc.Name}.{nameof(Receivers)}";
    }
}
