// <copyright file="Cords.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    public struct Cords : IEquatable<Cords>
    {
        /// <summary>
        /// Creates a new instance of the <see cref='Cords'/> class with member data left uninitialized.
        /// </summary>
        public static readonly Cords Empty;

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
        /// Initializes a new instance of the <see cref="Cords"/> class using coordinates specified by an integer value.
        /// </summary>
        public Cords(int dw)
        {
            X = LowInt16(dw);
            Y = HighInt16(dw);
        }

#if HAS_DRAWING
        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> class from a <see cref='System.Drawing.Point'/> .
        /// </summary>
        public Cords(System.Drawing.Point sz)
        {
            X = sz.X;
            Y = sz.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> class from a <see cref='System.Drawing.Size'/> .
        /// </summary>
        public Cords(System.Drawing.Size sz)
        {
            X = sz.Width;
            Y = sz.Height;
        }
#endif

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
#pragma warning disable CS0419 // cref 特性中有不明确的引用
        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> class from a <see cref='Windows.Foundation.Point'/> .
        /// </summary>
        public Cords(Windows.Foundation.Point sz)
        {
            X = unchecked((int)sz.X);
            Y = unchecked((int)sz.Y);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> class from a <see cref='Windows.Foundation.Size'/> .
        /// </summary>
        public Cords(Windows.Foundation.Size sz)
        {
            X = unchecked((int)sz.Width);
            Y = unchecked((int)sz.Height);
        }
#pragma warning restore CS0419 // cref 特性中有不明确的引用
#endif

        /// <summary>
        /// Gets a value indicating whether this <see cref='Cords'/> is empty.
        /// </summary>
        public readonly bool IsEmpty => X == 0 && Y == 0;

        /// <summary>
        /// Gets or sets the horizontal "X" coordinate.
        /// </summary>
        public int X { readonly get; set; }

        /// <summary>
        /// Gets or sets the vertical "Y" coordinate.
        /// </summary>
        public int Y { readonly get; set; }

#if HAS_DRAWING
        /// <summary>
        /// Creates a <see cref='System.Drawing.Point'/> with the coordinates of the specified <see cref='Cords'/>.
        /// </summary>
        /// <param name="p">The <see cref='Cords'/> to convert.</param>
        /// <returns>The <see cref='System.Drawing.Point'/> that results from the conversion.</returns>
        public static implicit operator System.Drawing.Point(Cords p) => new(p.X, p.Y);

        /// <summary>
        /// Creates a <see cref='System.Drawing.PointF'/> with the coordinates of the specified <see cref='Cords'/>.
        /// </summary>
        /// <param name="p">The <see cref='Cords'/> to convert.</param>
        /// <returns>The <see cref='System.Drawing.PointF'/> that results from the conversion.</returns>
        public static implicit operator System.Drawing.PointF(Cords p) => new(p.X, p.Y);

        /// <summary>
        /// Creates a <see cref='System.Drawing.Size'/> with the coordinates of the specified <see cref='Cords'/>.
        /// </summary>
        /// <param name="p">The <see cref='Cords'/> to convert.</param>
        /// <returns>The <see cref='System.Drawing.Size'/> that results from the conversion.</returns>
        public static explicit operator System.Drawing.Size(Cords p) => new(p.X, p.Y);

        /// <summary>
        /// Creates a <see cref='Cords'/> with the coordinates of the specified <see cref='System.Drawing.Point'/>.
        /// </summary>
        /// <param name="p">The <see cref='System.Drawing.Point'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> that results from the conversion.</returns>
        public static implicit operator Cords(System.Drawing.Point p) => new(p);

        /// <summary>
        /// Creates a <see cref='Cords'/> with the coordinates of the specified <see cref='System.Drawing.Size'/>.
        /// </summary>
        /// <param name="p">The <see cref='System.Drawing.Size'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> that results from the conversion.</returns>
        public static explicit operator Cords(System.Drawing.Size p) => new(p);

        /// <summary>
        /// Translates a <see cref='Cords'/> by a given <see cref='System.Drawing.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to translate.</param>
        /// <param name="sz">A <see cref='System.Drawing.Size'/> that specifies the pair of numbers
        /// to add to the coordinates of <paramref name="pt"/>.</param>
        /// <returns>The translated <see cref='Cords'/>.</returns>
        public static Cords operator +(Cords pt, System.Drawing.Size sz) => Add(pt, sz);

        /// <summary>
        /// Translates a <see cref='Cords'/> by the negative of a given <see cref='System.Drawing.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to translate.</param>
        /// <param name="sz">A <see cref='System.Drawing.Size'/> that specifies the pair of numbers
        /// to subtract from the coordinates of <paramref name="pt"/>.</param>
        /// <returns>A <see cref='Cords'/> structure that is translated by the negative of a given <see cref='System.Drawing.Size'/> structure.</returns>
        public static Cords operator -(Cords pt, System.Drawing.Size sz) => Subtract(pt, sz);
#endif

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
#pragma warning disable CS0419 // cref 特性中有不明确的引用
        /// <summary>
        /// Creates a <see cref='Windows.Foundation.Point'/> with the coordinates of the specified <see cref='Cords'/>.
        /// </summary>
        /// <param name="p">The <see cref='Cords'/> to convert.</param>
        /// <returns>The <see cref='Windows.Foundation.Point'/> that results from the conversion.</returns>
        public static implicit operator Windows.Foundation.Point(Cords p) => new(p.X, p.Y);

        /// <summary>
        /// Creates a <see cref='Windows.Foundation.Size'/> with the coordinates of the specified <see cref='Cords'/>.
        /// </summary>
        /// <param name="p">The <see cref='Cords'/> to convert.</param>
        /// <returns>The <see cref='Windows.Foundation.Size'/> that results from the conversion.</returns>
        public static explicit operator Windows.Foundation.Size(Cords p) => new(p.X, p.Y);

        /// <summary>
        /// Creates a <see cref='Cords'/> with the coordinates of the specified <see cref='Windows.Foundation.Point'/>.
        /// </summary>
        /// <param name="p">The <see cref='Windows.Foundation.Point'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> that results from the conversion.</returns>
        public static explicit operator Cords(Windows.Foundation.Point p) => new(p);

        /// <summary>
        /// Creates a <see cref='Cords'/> with the coordinates of the specified <see cref='Windows.Foundation.Size'/>.
        /// </summary>
        /// <param name="p">The <see cref='Windows.Foundation.Size'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> that results from the conversion.</returns>
        public static explicit operator Cords(Windows.Foundation.Size p) => new(p);

        /// <summary>
        /// Translates a <see cref='Cords'/> by a given <see cref='Windows.Foundation.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to translate.</param>
        /// <param name="sz">A <see cref='Windows.Foundation.Size'/> that specifies the pair of numbers
        /// to add to the coordinates of <paramref name="pt"/>.</param>
        /// <returns>The translated <see cref='Cords'/>.</returns>
        public static Cords operator +(Cords pt, Windows.Foundation.Size sz) => Add(pt, sz);

        /// <summary>
        /// Translates a <see cref='Cords'/> by the negative of a given <see cref='Windows.Foundation.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to translate.</param>
        /// <param name="sz">A <see cref='Windows.Foundation.Size'/> that specifies the pair of numbers
        /// to subtract from the coordinates of <paramref name="pt"/>.</param>
        /// <returns>A <see cref='Cords'/> structure that is translated by the negative of a given <see cref='Windows.Foundation.Size'/> structure.</returns>
        public static Cords operator -(Cords pt, Windows.Foundation.Size sz) => Subtract(pt, sz);
#pragma warning restore CS0419 // cref 特性中有不明确的引用
#endif

        /// <summary>
        /// Compares two <see cref='Cords'/> objects. The result specifies whether the values of the
        /// <see cref='X'/> and <see cref='Y'/> properties of the two
        /// <see cref='Cords'/> objects are equal.
        /// </summary>
        /// <param name="left">A <see cref='Cords'/> to compare.</param>
        /// <param name="right">A <see cref='Cords'/> to compare.</param>
        /// <returns><see langword="true"/> if the <see cref="X"/> and <see cref="Y"/> values
        /// of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Cords left, Cords right) => left.X == right.X && left.Y == right.Y;

        /// <summary>
        /// Compares two <see cref='Cords'/> objects. The result specifies whether the values of the
        /// <see cref='X'/> or <see cref='Y'/> properties of the two
        /// <see cref='Cords'/> objects are unequal.
        /// </summary>
        /// <param name="left">A <see cref='Cords'/> to compare.</param>
        /// <param name="right">A <see cref='Cords'/> to compare.</param>
        /// <returns><see langword="true"/> if the values of either the <see cref="X"/> or <see cref="Y"/> values
        /// of <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Cords left, Cords right) => !(left == right);

#if HAS_DRAWING
        /// <summary>
        /// Translates a <see cref='Cords'/> by a given <see cref='System.Drawing.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to add.</param>
        /// <param name="sz">The <see cref='System.Drawing.Size'/> to add.</param>
        /// <returns>The <see cref='Cords'/> that is the result of the addition operation.</returns>
        public static Cords Add(Cords pt, System.Drawing.Size sz) => new(unchecked(pt.X + sz.Width), unchecked(pt.Y + sz.Height));

        /// <summary>
        /// Translates a <see cref='Cords'/> by the negative of a given <see cref='System.Drawing.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to be subtracted from.</param>
        /// <param name="sz">The <see cref='System.Drawing.Size'/> to subtract from the Point.</param>
        /// <returns>The <see cref='Cords'/> that is the result of the subtraction operation.</returns>
        public static Cords Subtract(Cords pt, System.Drawing.Size sz) => new(unchecked(pt.X - sz.Width), unchecked(pt.Y - sz.Height));

        /// <summary>
        /// Converts a <see cref='System.Drawing.PointF'/> to a <see cref='Cords'/> by performing a ceiling operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref='System.Drawing.PointF'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> this method converts to.</returns>
        public static Cords Ceiling(System.Drawing.PointF value) => new(unchecked((int)Math.Ceiling(value.X)), unchecked((int)Math.Ceiling(value.Y)));

        /// <summary>
        /// Converts a <see cref='System.Drawing.PointF'/> to a <see cref='Cords'/> by performing a truncate operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref='System.Drawing.PointF'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> this method converts to.</returns>
        public static Cords Truncate(System.Drawing.PointF value) => new(unchecked((int)value.X), unchecked((int)value.Y));

        /// <summary>
        /// Converts a <see cref='System.Drawing.PointF'/> to a <see cref='Cords'/> by performing a round operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref='System.Drawing.PointF'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> this method converts to.</returns>
        public static Cords Round(System.Drawing.PointF value) => new(unchecked((int)Math.Round(value.X)), unchecked((int)Math.Round(value.Y)));
#endif

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
#pragma warning disable CS0419 // cref 特性中有不明确的引用
        /// <summary>
        /// Translates a <see cref='Cords'/> by a given <see cref='Windows.Foundation.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to add.</param>
        /// <param name="sz">The <see cref='Windows.Foundation.Size'/> to add.</param>
        /// <returns>The <see cref='Cords'/> that is the result of the addition operation.</returns>
        public static Cords Add(Cords pt, Windows.Foundation.Size sz) => new(unchecked((int)(pt.X + sz.Width)), unchecked((int)(pt.Y + sz.Height)));

        /// <summary>
        /// Translates a <see cref='Cords'/> by the negative of a given <see cref='Windows.Foundation.Size'/>.
        /// </summary>
        /// <param name="pt">The <see cref='Cords'/> to be subtracted from.</param>
        /// <param name="sz">The <see cref='Windows.Foundation.Size'/> to subtract from the Point.</param>
        /// <returns>The <see cref='Cords'/> that is the result of the subtraction operation.</returns>
        public static Cords Subtract(Cords pt, Windows.Foundation.Size sz) => new(unchecked((int)(pt.X - sz.Width)), unchecked((int)(pt.Y - sz.Height)));

        /// <summary>
        /// Converts a <see cref='Windows.Foundation.Point'/> to a <see cref='Cords'/> by performing a ceiling operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref='Windows.Foundation.Point'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> this method converts to.</returns>
        public static Cords Ceiling(Windows.Foundation.Point value) => new(unchecked((int)Math.Ceiling(value.X)), unchecked((int)Math.Ceiling(value.Y)));

        /// <summary>
        /// Converts a <see cref='Windows.Foundation.Point'/> to a <see cref='Cords'/> by performing a truncate operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref='Windows.Foundation.Point'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> this method converts to.</returns>
        public static Cords Truncate(Windows.Foundation.Point value) => new(unchecked((int)value.X), unchecked((int)value.Y));

        /// <summary>
        /// Converts a <see cref='Windows.Foundation.Point'/> to a <see cref='Cords'/> by performing a round operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref='Windows.Foundation.Point'/> to convert.</param>
        /// <returns>The <see cref='Cords'/> this method converts to.</returns>
        public static Cords Round(Windows.Foundation.Point value) => new(unchecked((int)Math.Round(value.X)), unchecked((int)Math.Round(value.Y)));
#pragma warning restore CS0419 // cref 特性中有不明确的引用
#endif

        /// <summary>
        /// Specifies whether this <see cref='Cords'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref='Cords'/> and has the same coordinates as this point instance.</returns>
        public override readonly bool Equals(object obj) => obj is Cords cords && Equals(cords);

        /// <summary>
        /// Specifies whether this <see cref='Cords'/> contains the same coordinates as the specified
        /// <see cref='object'/>.
        /// </summary>
        /// <param name="other">The point to test for equality.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> has the same coordinates as this point instance.</returns>
        public readonly bool Equals(Cords other) => this == other;

        /// <summary>
        /// Returns a hash code for this <see cref="Cords"/>.
        /// </summary>
        /// <returns>An integer value that specifies a hash value for this <see cref="Cords"/>.</returns>
        public override readonly int GetHashCode() =>
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            HashCode.Combine(X, Y);
#else
            X ^ Y;
#endif

        /// <summary>
        /// Translates this <see cref='Cords'/> by the specified amount.
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
        /// Translates this <see cref='Cords'/> by the specified amount.
        /// </summary>
        /// <param name="p">The <see cref='Cords'/> used offset this <see cref='Cords'/>.</param>
        public void Offset(Cords p) => Offset(p.X, p.Y);

        /// <summary>
        /// Converts this <see cref='Cords'/> to a human readable string.
        /// </summary>
        /// <returns>A string that represents this <see cref='Cords'/>.</returns>
        public override readonly string ToString() => $"{{X={X},Y={Y}}}";

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

        private static short HighInt16(int n) => unchecked((short)((n >> 16) & 0xffff));

        private static short LowInt16(int n) => unchecked((short)(n & 0xffff));
    }
}
