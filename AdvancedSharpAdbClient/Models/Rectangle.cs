#if !HAS_DRAWING
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Stores the location and size of a rectangular region.
    /// </summary>
    /// <param name="x">The x-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public struct Rectangle(int x, int y, int width, int height) : IEquatable<Rectangle>
    {
        /// <summary>
        /// Represents a <see cref="Rectangle"/> structure with its properties left uninitialized.
        /// </summary>
        public static readonly Rectangle Empty;

        /// <summary>
        /// Creates a new <see cref='Rectangle'/> with the specified location and size.
        /// </summary>
        /// <param name="left">The x-coordinate of the upper-left corner of this <see cref='Rectangle'/> structure.</param>
        /// <param name="top">The y-coordinate of the upper-left corner of this <see cref='Rectangle'/> structure.</param>
        /// <param name="right">The x-coordinate of the lower-right corner of this <see cref='Rectangle'/> structure.</param>
        /// <param name="bottom">The y-coordinate of the lower-right corner of this <see cref='Rectangle'/> structure.</param>
        /// <returns>The new <see cref="Rectangle"/> that this method creates.</returns>
        public static Rectangle FromLTRB(int left, int top, int right, int bottom) =>
            new(left, top, unchecked(right - left), unchecked(bottom - top));

        /// <summary>
        /// Gets or sets the coordinates of the upper-left corner of the rectangular region represented by this
        /// <see cref='Rectangle'/>.
        /// </summary>
        public Point Location
        {
            readonly get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Rectangle'/>.
        /// </summary>
        public int X { readonly get; set; } = x;

        /// <summary>
        /// Gets or sets the y-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Rectangle'/>.
        /// </summary>
        public int Y { readonly get; set; } = y;

        /// <summary>
        /// Gets or sets the width of the rectangular region defined by this <see cref='Rectangle'/>.
        /// </summary>
        public int Width { readonly get; set; } = width;

        /// <summary>
        /// Gets or sets the width of the rectangular region defined by this <see cref='Rectangle'/>.
        /// </summary>
        public int Height { readonly get; set; } = height;

        /// <summary>
        /// Gets the x-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Rectangle'/> .
        /// </summary>
        public readonly int Left => X;

        /// <summary>
        /// Gets the y-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Rectangle'/>.
        /// </summary>
        public readonly int Top => Y;

        /// <summary>
        /// Gets the x-coordinate of the lower-right corner of the rectangular region defined by this
        /// <see cref='Rectangle'/>.
        /// </summary>
        public readonly int Right => unchecked(X + Width);

        /// <summary>
        /// Gets the y-coordinate of the lower-right corner of the rectangular region defined by this
        /// <see cref='Rectangle'/>.
        /// </summary>
        public readonly int Bottom => unchecked(Y + Height);

        /// <summary>
        /// Tests whether this <see cref='Rectangle'/> has a <see cref='Rectangle.Width'/>
        /// or a <see cref='Rectangle.Height'/> of 0.
        /// </summary>
        public readonly bool IsEmpty => Height == 0 && Width == 0 && X == 0 && Y == 0;

        /// <summary>
        /// Tests whether <paramref name="obj"/> is a <see cref='Rectangle'/> with the same location
        /// and size of this Rectangle.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if <paramref name="obj"/> is a <see cref="Rectangle"/> structure
        /// and its <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties are equal to
        /// the corresponding properties of this <see cref="Rectangle"/> structure; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj) => obj is Rectangle rectangle && Equals(rectangle);

        /// <inheritdoc/>
        public readonly bool Equals(Rectangle other) => this == other;

        /// <summary>
        /// Tests whether two <see cref='Rectangle'/> objects have equal location and size.
        /// </summary>
        /// <param name="left">The Rectangle structure that is to the left of the equality operator.</param>
        /// <param name="right">The Rectangle structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="Rectangle"/> structures have equal
        /// <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties.</returns>
        public static bool operator ==(Rectangle left, Rectangle right) =>
            left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;

        /// <summary>
        /// Tests whether two <see cref='Rectangle'/> objects differ in location or size.
        /// </summary>
        /// <param name="left">The Rectangle structure that is to the left of the inequality operator.</param>
        /// <param name="right">The Rectangle structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if any of the <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>
        /// properties of the two <see cref="Rectangle"/> structures are unequal; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(Rectangle left, Rectangle right) => !(left == right);

        /// <summary>
        /// Determines if the specified point is contained within the rectangular region defined by this
        /// <see cref='Rectangle'/> .
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <returns>This method returns <see langword="true"/> if the point defined by <paramref name="x"/> and <paramref name="y"/>
        /// is contained within this <see cref="Rectangle"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(int x, int y) => X <= x && x < X + Width && Y <= y && y < Y + Height;

        /// <summary>
        /// Determines if the specified point is contained within the rectangular region defined by this
        /// <see cref='Rectangle'/> .
        /// </summary>
        /// <param name="pt">The <see cref="Point"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if the point represented by <paramref name="pt"/>
        /// is contained within this <see cref="Rectangle"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(Point pt) => Contains(pt.X, pt.Y);

        /// <summary>
        /// Determines if the rectangular region represented by <paramref name="rect"/> is entirely contained within the
        /// rectangular region represented by this <see cref='Rectangle'/> .
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if the rectangular region represented by <paramref name="rect"/>
        /// is entirely contained within this <see cref="Rectangle"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(Rectangle rect) =>
            (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
            (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);

        /// <summary>
        /// Returns the hash code for this <see cref="Rectangle"/> structure.
        /// </summary>
        /// <returns>An integer that represents the hash code for this rectangle.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        /// <summary>
        /// Inflates this <see cref='Rectangle'/> by the specified amount.
        /// </summary>
        /// <param name="width">The amount to inflate this <see cref="Rectangle"/> horizontally.</param>
        /// <param name="height">The amount to inflate this <see cref="Rectangle"/> vertically.</param>
        public void Inflate(int width, int height)
        {
            unchecked
            {
                X -= width;
                Y -= height;

                Width += 2 * width;
                Height += 2 * height;
            }
        }

        /// <summary>
        /// Creates a <see cref='Rectangle'/> that is inflated by the specified amount.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> with which to start. This rectangle is not modified.</param>
        /// <param name="x">The amount to inflate this <see cref="Rectangle"/> horizontally.</param>
        /// <param name="y">The amount to inflate this <see cref="Rectangle"/> vertically.</param>
        public static Rectangle Inflate(Rectangle rect, int x, int y)
        {
            Rectangle r = rect;
            r.Inflate(x, y);
            return r;
        }

        /// <summary>
        /// Creates a Rectangle that represents the intersection between this Rectangle and rect.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> with which to intersect.</param>
        public void Intersect(Rectangle rect)
        {
            Rectangle result = Intersect(rect, this);

            X = result.X;
            Y = result.Y;
            Width = result.Width;
            Height = result.Height;
        }

        /// <summary>
        /// Creates a rectangle that represents the intersection between a and b. If there is no intersection, an
        /// empty rectangle is returned.
        /// </summary>
        /// <param name="a">A rectangle to intersect.</param>
        /// <param name="b">A rectangle to intersect.</param>
        /// <returns>A <see cref="Rectangle"/> that represents the intersection of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static Rectangle Intersect(Rectangle a, Rectangle b)
        {
            int x1 = Math.Max(a.X, b.X);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Max(a.Y, b.Y);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            return x2 >= x1 && y2 >= y1 ? new Rectangle(x1, y1, x2 - x1, y2 - y1) : Empty;
        }

        /// <summary>
        /// Determines if this rectangle intersects with rect.
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <returns>This method returns <see langword="true"/> if there is any intersection, otherwise <see langword="false"/>.</returns>
        public readonly bool IntersectsWith(Rectangle rect) =>
            (rect.X < X + Width) && (X < rect.X + rect.Width) &&
            (rect.Y < Y + Height) && (Y < rect.Y + rect.Height);

        /// <summary>
        /// Creates a rectangle that represents the union between a and b.
        /// </summary>
        /// <param name="a">A rectangle to union.</param>
        /// <param name="b">A rectangle to union.</param>
        /// <returns>A <see cref="Rectangle"/> structure that bounds the union of the two <see cref="Rectangle"/> structures.</returns>
        public static Rectangle Union(Rectangle a, Rectangle b)
        {
            int x1 = Math.Min(a.X, b.X);
            int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Min(a.Y, b.Y);
            int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        /// <summary>
        /// Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="pos">Amount to offset the location.</param>
        public void Offset(Point pos) => Offset(pos.X, pos.Y);

        /// <summary>
        /// Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="x">The horizontal offset.</param>
        /// <param name="y">The vertical offset.</param>
        public void Offset(int x, int y)
        {
            unchecked
            {
                X += x;
                Y += y;
            }
        }

        /// <summary>
        /// Converts the attributes of this <see cref='Rectangle'/> to a human readable string.
        /// </summary>
        /// <returns>A string that contains the position, width, and height of this <see cref="Rectangle"/> structure ¾
        /// for example, <c>{X=20, Y=20, Width=100, Height=50}</c>.</returns>
        public override readonly string ToString() => $"{{X={X},Y={Y},Width={Width},Height={Height}}}";
    }
}
#endif