// <copyright file="Cords.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace AdvancedSharpAdbClient.DeviceCommands.Models
{
    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    /// <remarks>This is a host of type <see cref="System.Drawing.Point"/>.
    /// Using <see cref="Point"/> to get the <see cref="System.Drawing.Point"/> value.</remarks>
    public struct Cords : IEquatable<Cords>, IEquatable<Point>
    {
        /// <summary>
        /// Creates a new instance of the <see cref='Cords'/> struct with member data left uninitialized.
        /// </summary>
        public static readonly Cords Empty;

        /// <summary>
        /// The <see cref="Point"/> that represents the coordinates of this <see cref="Cords"/>.
        /// </summary>
        private Point point;

        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> struct.
        /// </summary>
        public Cords() : this(new Point()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> struct.
        /// </summary>
        /// <param name="point">A <see cref='System.Drawing.Point'/> that specifies the coordinates for the new <see cref='Cords'/>.</param>
#if HAS_DRAWING
        public
#else
        internal
#endif
            Cords(Point point) : this(in point) { }

        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> struct.
        /// </summary>
        /// <param name="point">A <see cref='System.Drawing.Point'/> that specifies the coordinates for the new <see cref='Cords'/>.</param>
        internal Cords(in Point point) => this.point = point;

        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> struct with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal "X" coordinate.</param>
        /// <param name="y">The vertical "Y" coordinate.</param>
        public Cords(int x, int y) : this(new Point(x, y)) { }

        /// <summary>
        /// Initializes a new instance of the Point class using coordinates specified by an integer value.
        /// </summary>
        public Cords(int dw) : this(new Point(dw)) { }

        /// <summary>
        /// Gets a value indicating whether this <see cref='System.Drawing.Point'/> is empty.
        /// </summary>
        public readonly bool IsEmpty => point.IsEmpty;

        /// <summary>
        /// Gets or sets the <see cref="Point"/> that represents the coordinates of this <see cref="Cords"/>.
        /// </summary>
#if HAS_DRAWING
        public
#else
        internal
#endif
            Point Point
        {
            readonly get => point;
            set => point = value;
        }

        /// <summary>
        /// Gets the x-coordinate of this <see cref='System.Drawing.Point'/>.
        /// </summary>
        public int X
        {
            readonly get => point.X;
            set => point.X = value;
        }

        /// <summary>
        /// Gets the y-coordinate of this <see cref='System.Drawing.Point'/>.
        /// </summary>
        public int Y
        {
            readonly get => point.Y;
            set => point.Y = value;
        }

        /// <summary>
        /// Compares two <see cref='Cords'/> objects. The result specifies whether the values of the
        /// <see cref='X'/> and <see cref='Y'/> properties of the two
        /// <see cref='Cords'/> objects are equal.
        /// </summary>
        /// <param name="left">A <see cref='Cords'/> to compare.</param>
        /// <param name="right">A <see cref='Cords'/> to compare.</param>
        /// <returns><see langword="true"/> if the <see cref="X"/> and <see cref="Y"/> values
        /// of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Cords left, Cords right) => left.point == right.point;

        /// <summary>
        /// Compares two <see cref='Cords'/> objects. The result specifies whether the values of the
        /// <see cref='X'/> or <see cref='Y'/> properties of the two
        /// <see cref='Cords'/>  objects are unequal.
        /// </summary>
        /// <param name="left">A <see cref='Cords'/> to compare.</param>
        /// <param name="right">A <see cref='Cords'/> to compare.</param>
        /// <returns><see langword="true"/> if the values of either the <see cref="X"/> or <see cref="Y"/> values
        /// of <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Cords left, Cords right) => left.point != right.point;

        /// <summary>
        /// Specifies whether this <see cref='Cords'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref='Cords'/> and has the same coordinates as this point instance.</returns>
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => (obj is Cords cords && Equals(cords)) || (obj is Point point && this.point.Equals(point));

        /// <summary>
        /// Specifies whether this <see cref='System.Drawing.Point'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="other">The point to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same coordinates as this point instance.</returns>
        public readonly bool Equals(Cords other) => point.Equals(other.point);

        /// <summary>
        /// Specifies whether this <see cref='System.Drawing.Point'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="other">The point to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same coordinates as this point instance.</returns>
#if HAS_DRAWING
        public
#else
        internal
#endif
            readonly bool Equals(Point other) => point.Equals(other);

        /// <summary>
        /// Returns a hash code.
        /// </summary>
        /// <returns>An integer value that specifies a hash value for this <see cref="Point"/>.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(point);

        /// <summary>
        /// Translates this <see cref='System.Drawing.Point'/> by the specified amount.
        /// </summary>
        /// <param name="dx">The amount to offset the x-coordinate.</param>
        /// <param name="dy">The amount to offset the y-coordinate.</param>
        public void Offset(int dx, int dy) => point.Offset(dx, dy);

#if HAS_DRAWING
        /// <summary>
        /// Creates a <see cref='System.Drawing.Point'/> with the coordinates of the specified <see cref='Cords'/>.
        /// </summary>
        /// <param name="p">The <see cref='Cords'/> to convert.</param>
        /// <returns>The <see cref='System.Drawing.Point'/> that results from the conversion.</returns>
        public static implicit operator Point(Cords p) => p.point;

        /// <summary>
        /// Creates a <see cref='Cords'/> with the coordinates of the specified <see cref='System.Drawing.Point'/>.
        /// </summary>
        /// <param name="p">The <see cref='System.Drawing.Point'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> that results from the conversion.</returns>
        public static implicit operator Cords(Point p) => new(in p);
#else
        /// <summary>
        /// Specifies whether this <see cref='System.Drawing.Point'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="other">The point to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same coordinates as this point instance.</returns>
        readonly bool IEquatable<Point>.Equals(Point other) => Equals(other);
#endif

        /// <summary>
        /// Translates this <see cref='System.Drawing.Point'/> by the specified amount.
        /// </summary>
        /// <param name="p">The <see cref='System.Drawing.Point'/> used offset this <see cref='System.Drawing.Point'/>.</param>
#if HAS_DRAWING
        public
#else
        internal
#endif
            void Offset(Point p) => point.Offset(p);

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
        /// Converts this <see cref='System.Drawing.Point'/> to a human readable string.
        /// </summary>
        /// <returns>A string that represents this <see cref='System.Drawing.Point'/>.</returns>
        public override readonly string ToString() => point.ToString();
    }
}
