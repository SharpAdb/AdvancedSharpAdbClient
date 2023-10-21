#if !HAS_DRAWING
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Represents an ordered pair of x and y coordinates that define a point in a two-dimensional plane.
    /// </summary>
    public struct Point : IEquatable<Point>
    {
        /// <summary>
        /// Creates a new instance of the <see cref='Point'/> class with member data left uninitialized.
        /// </summary>
        public static readonly Point Empty;

        private int x; // Do not rename (binary serialization)
        private int y; // Do not rename (binary serialization)

        /// <summary>
        /// Initializes a new instance of the <see cref='Point'/> class with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal "X" coordinate.</param>
        /// <param name="y">The vertical "Y" coordinate.</param>
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Initializes a new instance of the Point class using coordinates specified by an integer value.
        /// </summary>
        public Point(int dw)
        {
            x = LowInt16(dw);
            y = HighInt16(dw);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref='Point'/> is empty.
        /// </summary>
        public readonly bool IsEmpty => x == 0 && y == 0;

        /// <summary>
        /// Gets the x-coordinate of this <see cref='Point'/>.
        /// </summary>
        public int X
        {
            readonly get => x;
            set => x = value;
        }

        /// <summary>
        /// Gets the y-coordinate of this <see cref='Point'/>.
        /// </summary>
        public int Y
        {
            readonly get => y;
            set => y = value;
        }

        /// <summary>
        /// Compares two <see cref='Point'/> objects. The result specifies whether the values of the
        /// <see cref='Point.X'/> and <see cref='Point.Y'/> properties of the two
        /// <see cref='Point'/> objects are equal.
        /// </summary>
        /// <param name="left">A <see cref='Point'/> to compare.</param>
        /// <param name="right">A <see cref='Point'/> to compare.</param>
        /// <returns><see langword="true"/> if the <see cref="X"/> and <see cref="Y"/> values
        /// of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Point left, Point right) => left.X == right.X && left.Y == right.Y;

        /// <summary>
        /// Compares two <see cref='Point'/> objects. The result specifies whether the values of the
        /// <see cref='Point.X'/> or <see cref='Point.Y'/> properties of the two
        /// <see cref='Point'/>  objects are unequal.
        /// </summary>
        /// <param name="left">A <see cref='Point'/> to compare.</param>
        /// <param name="right">A <see cref='Point'/> to compare.</param>
        /// <returns><see langword="true"/> if the values of either the <see cref="X"/> or <see cref="Y"/> values
        /// of <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Point left, Point right) => !(left == right);

        /// <summary>
        /// Specifies whether this <see cref='Point'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref='Point'/> and has the same coordinates as this point instance.</returns>
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Point point && Equals(point);

        /// <summary>
        /// Specifies whether this <see cref='Point'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="other">The point to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same coordinates as this point instance.</returns>
        public readonly bool Equals(Point other) => this == other;

        /// <summary>
        /// Returns a hash code.
        /// </summary>
        /// <returns>An integer value that specifies a hash value for this <see cref="Point"/>.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Translates this <see cref='Point'/> by the specified amount.
        /// </summary>
        /// <param name="dx">The amount to offset the x-coordinate.</param>
        /// <param name="dy">The amount to offset the y-coordinate.</param>
        public void Offset(int dx, int dy)
        {
            unchecked
            {
                X += dx;
                Y += dy;
            }
        }

        /// <summary>
        /// Translates this <see cref='Point'/> by the specified amount.
        /// </summary>
        /// <param name="p">The <see cref='Point'/> used offset this <see cref='Point'/>.</param>
        public void Offset(Point p) => Offset(p.X, p.Y);

        /// <summary>
        /// Deconstruct the <see cref="Point"/> class.
        /// </summary>
        /// <param name="cx">The horizontal "X" coordinate.</param>
        /// <param name="cy">The vertical "Y" coordinate.</param>
        public readonly void Deconstruct(out int cx, out int cy)
        {
            cx = X;
            cy = Y;
        }

        /// <summary>
        /// Converts this <see cref='Point'/> to a human readable string.
        /// </summary>
        /// <returns>A string that represents this <see cref='Point'/>.</returns>
        public override readonly string ToString() => $"{{X={X},Y={Y}}}";

        private static short HighInt16(int n) => unchecked((short)((n >> 16) & 0xffff));

        private static short LowInt16(int n) => unchecked((short)(n & 0xffff));
    }
}
#endif