// <copyright file="NamespaceDoc.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.ComponentModel;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// The classes in this namespace provide models for <see cref="AdvancedSharpAdbClient"/>.
    /// </summary>
    /// <remarks><c>Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.</c></remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal abstract class NamespaceDoc : AdvancedSharpAdbClient.NamespaceDoc
    {
        /// <summary>
        /// The name of the namespace <see cref="Models"/>.
        /// </summary>
        public new const string Name = $"{AdvancedSharpAdbClient.NamespaceDoc.Name}.{nameof(Models)}";
    }
}
