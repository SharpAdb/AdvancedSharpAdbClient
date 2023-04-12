// <copyright file="Cords.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Contains element coordinates.
    /// </summary>
    public struct Cords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cords"/> class.
        /// </summary>
        /// <param name="cx">The horizontal "X" coordinate.</param>
        /// <param name="cy">The vertical "Y" coordinate.</param>
        public Cords(int cx, int cy)
        {
            X = cx;
            Y = cy;
        }

        /// <summary>
        /// Gets or sets the horizontal "X" coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the vertical "Y" coordinate.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Deconstruct the <see cref="Cords"/> class.
        /// </summary>
        /// <param name="cx">The horizontal "X" coordinate.</param>
        /// <param name="cy">The vertical "Y" coordinate.</param>
        public readonly void Deconstruct(out int cx, out int cy)
        {
            cx = X;
            cy = Y;
        }
    }
}
