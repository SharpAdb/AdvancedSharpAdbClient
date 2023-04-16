// <copyright file="Cords.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Contains element coordinates.
    /// </summary>
    public sealed class Cords
    {
        internal AdvancedSharpAdbClient.Cords cords;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cords"/> class.
        /// </summary>
        /// <param name="cx">The horizontal "X" coordinate.</param>
        /// <param name="cy">The vertical "Y" coordinate.</param>
        public Cords(int cx, int cy) => cords = new(cx, cy);

        internal Cords(AdvancedSharpAdbClient.Cords cords) => this.cords = cords;

        internal static Cords GetCords(AdvancedSharpAdbClient.Cords cords) => new(cords);

        /// <summary>
        /// Gets or sets the horizontal "X" coordinate.
        /// </summary>
        public int X
        {
            get => cords.X;
            set => cords.X = value;
        }

        /// <summary>
        /// Gets or sets the vertical "Y" coordinate.
        /// </summary>
        public int Y
        {
            get => cords.Y;
            set => cords.Y = value;
        }

        /// <summary>
        /// Deconstruct the <see cref="Cords"/> class.
        /// </summary>
        /// <param name="cx">The horizontal "X" coordinate.</param>
        /// <param name="cy">The vertical "Y" coordinate.</param>
        public void Deconstruct(out int cx, out int cy) => cords.Deconstruct(out cx, out cy);
    }
}
