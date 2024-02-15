// <copyright file="Area.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace AdvancedSharpAdbClient.DeviceCommands.Models
{
    /// <summary>
    /// Stores the location and size of a rectangular region.
    /// </summary>
    /// <remarks>This is a host of type <see cref="System.Drawing.Rectangle"/>.
    /// Using <see cref="Rectangle"/> to get the <see cref="System.Drawing.Rectangle"/> value.</remarks>
    [DebuggerDisplay($"{nameof(Area)} \\{{ {nameof(X)} = {{{nameof(X)}}}, {nameof(Y)} = {{{nameof(Y)}}}, {nameof(Width)} = {{{nameof(Width)}}}, {nameof(Height)} = {{{nameof(Height)}}} }}")]
    public struct Area : IEquatable<Area>, IEquatable<Rectangle>
    {
        /// <summary>
        /// Represents a <see cref="Area"/> structure with its properties left uninitialized.
        /// </summary>
        public static readonly Area Empty;

        /// <summary>
        /// The <see cref="Rectangle"/> that represents the location and size of this <see cref="Area"/>.
        /// </summary>
        internal Rectangle rectangle;

        /// <summary>
        /// Initializes a new instance of the <see cref='Cords'/> struct.
        /// </summary>
        public Area() : this(new Rectangle()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> struct.
        /// </summary>
        /// <param name="rectangle">A <see cref='Rectangle'/> that specifies the location and size for the new <see cref='Area'/>.</param>
#if HAS_DRAWING
        public
#else
        internal
#endif
            Area(in Rectangle rectangle) => this.rectangle = rectangle;

        ///<summary>
        /// Initializes a new instance of the <see cref="Area"/> struct with the specified location and size.
        ///</summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public Area(int x, int y, int width, int height) => rectangle = new Rectangle(x, y, width, height);

        /// <summary>
        /// Creates a new <see cref='Area'/> with the specified location and size.
        /// </summary>
        /// <param name="left">The x-coordinate of the upper-left corner of this <see cref='Area'/> structure.</param>
        /// <param name="top">The y-coordinate of the upper-left corner of this <see cref='Area'/> structure.</param>
        /// <param name="right">The x-coordinate of the lower-right corner of this <see cref='Area'/> structure.</param>
        /// <param name="bottom">The y-coordinate of the lower-right corner of this <see cref='Area'/> structure.</param>
        /// <returns>The new <see cref="Area"/> that this method creates.</returns>
        public static Area FromLTRB(int left, int top, int right, int bottom) => new(Rectangle.FromLTRB(left, top, right, bottom));

        /// <summary>
        /// Gets or sets the <see cref="Rectangle"/> that represents the location and size of this <see cref="Area"/>.
        /// </summary>
#if HAS_DRAWING
        public
#else
        internal
#endif
            Rectangle Rectangle
        {
            readonly get => rectangle;
            set => rectangle = value;
        }

        /// <summary>
        /// Gets or sets the coordinates of the upper-left corner of the rectangular region represented by this
        /// <see cref='Area'/>.
        /// </summary>
        public Cords Location
        {
            readonly get => new(rectangle.Location);
            set => rectangle.Location = value.Point;
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public int X
        {
            readonly get => rectangle.X;
            set => rectangle.X = value;
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public int Y
        {
            readonly get => rectangle.Y;
            set => rectangle.Y = value;
        }

        /// <summary>
        /// Gets or sets the width of the rectangular region defined by this <see cref='Area'/>.
        /// </summary>
        public int Width
        {
            readonly get => rectangle.Width;
            set => rectangle.Width = value;
        }

        /// <summary>
        /// Gets or sets the width of the rectangular region defined by this <see cref='Area'/>.
        /// </summary>
        public int Height
        {
            readonly get => rectangle.Height;
            set => rectangle.Height = value;
        }

        /// <summary>
        /// Gets the x-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/> .
        /// </summary>
        public readonly int Left => rectangle.Left;

        /// <summary>
        /// Gets the y-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public readonly int Top => rectangle.Top;

        /// <summary>
        /// Gets the x-coordinate of the lower-right corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public readonly int Right => rectangle.Right;

        /// <summary>
        /// Gets the y-coordinate of the lower-right corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public readonly int Bottom => rectangle.Bottom;

        /// <summary>
        /// Tests whether this <see cref='Area'/> has a <see cref='Width'/>
        /// or a <see cref='Height'/> of 0.
        /// </summary>
        public readonly bool IsEmpty => rectangle.IsEmpty;

        /// <summary>
        /// Tests whether <paramref name="obj"/> is a <see cref='Area'/> with the same location
        /// and size of this Rectangle.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if <paramref name="obj"/> is a <see cref="Rectangle"/> structure
        /// and its <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties are equal to
        /// the corresponding properties of this <see cref="Area"/> structure; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => (obj is Area area && Equals(area)) || (obj is Rectangle rectangle && rectangle.Equals(rectangle));

        /// <inheritdoc/>
        public readonly bool Equals(Area other) => rectangle.Equals(other.rectangle);

        /// <inheritdoc/>
#if HAS_DRAWING
        public
#else
        internal
#endif
            readonly bool Equals(Rectangle other) => rectangle.Equals(other);

        /// <summary>
        /// Tests whether two <see cref='Area'/> objects have equal location and size.
        /// </summary>
        /// <param name="left">The <see cref="Area"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="Area"/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="Area"/> structures have equal
        /// <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties.</returns>
        public static bool operator ==(Area left, Area right) => left.rectangle == right.rectangle;

        /// <summary>
        /// Tests whether two <see cref='Area'/> objects differ in location or size.
        /// </summary>
        /// <param name="left">The <see cref="Area"/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="Area"/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if any of the <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>
        /// properties of the two <see cref="Area"/> structures are unequal; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(Area left, Area right) => left.rectangle != right.rectangle;

        /// <summary>
        /// Determines if the specified point is contained within the rectangular region defined by this
        /// <see cref='Area'/> .
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <returns>This method returns <see langword="true"/> if the point defined by <paramref name="x"/> and <paramref name="y"/>
        /// is contained within this <see cref="Area"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(int x, int y) => rectangle.Contains(x, y);

        /// <summary>
        /// Determines if the specified point is contained within the rectangular region defined by this
        /// <see cref='Area'/> .
        /// </summary>
        /// <param name="pt">The <see cref="Cords"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if the point represented by <paramref name="pt"/>
        /// is contained within this <see cref="Area"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(Cords pt) => rectangle.Contains(pt.Point);

        /// <summary>
        /// Determines if the rectangular region represented by <paramref name="rect"/> is entirely contained within the
        /// rectangular region represented by this <see cref='Area'/> .
        /// </summary>
        /// <param name="rect">The <see cref="Area"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if the rectangular region represented by <paramref name="rect"/>
        /// is entirely contained within this <see cref="Area"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(Area rect) => rectangle.Contains(rect.rectangle);

        /// <summary>
        /// Returns the hash code for this <see cref="Area"/> structure.
        /// </summary>
        /// <returns>An integer that represents the hash code for this rectangle.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(rectangle);

        /// <summary>
        /// Inflates this <see cref='Area'/> by the specified amount.
        /// </summary>
        /// <param name="width">The amount to inflate this <see cref="Area"/> horizontally.</param>
        /// <param name="height">The amount to inflate this <see cref="Area"/> vertically.</param>
        public void Inflate(int width, int height) => rectangle.Inflate(width, height);

        /// <summary>
        /// Creates a <see cref='Area'/> that is inflated by the specified amount.
        /// </summary>
        /// <param name="rect">The <see cref="Area"/> with which to start. This rectangle is not modified.</param>
        /// <param name="x">The amount to inflate this <see cref="Area"/> horizontally.</param>
        /// <param name="y">The amount to inflate this <see cref="Area"/> vertically.</param>
        public static Area Inflate(Area rect, int x, int y)
        {
            Area r = rect;
            r.Inflate(x, y);
            return r;
        }

        /// <summary>
        /// Creates a Rectangle that represents the intersection between this Rectangle and rect.
        /// </summary>
        /// <param name="rect">The <see cref="Area"/> with which to intersect.</param>
        public void Intersect(Area rect) => rectangle.Intersect(rect.rectangle);

        /// <summary>
        /// Creates a rectangle that represents the intersection between a and b. If there is no intersection, an
        /// empty rectangle is returned.
        /// </summary>
        /// <param name="a">A rectangle to intersect.</param>
        /// <param name="b">A rectangle to intersect.</param>
        /// <returns>A <see cref="Area"/> that represents the intersection of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static Area Intersect(Area a, Area b)
        {
            Rectangle result = Rectangle.Intersect(a.rectangle, b.rectangle);
            return new Area(result);
        }

        /// <summary>
        /// Determines if this rectangle intersects with rect.
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <returns>This method returns <see langword="true"/> if there is any intersection, otherwise <see langword="false"/>.</returns>
        public readonly bool IntersectsWith(Area rect) => rectangle.IntersectsWith(rect.rectangle);

        /// <summary>
        /// Creates a rectangle that represents the union between a and b.
        /// </summary>
        /// <param name="a">A rectangle to union.</param>
        /// <param name="b">A rectangle to union.</param>
        /// <returns>A <see cref="Area"/> structure that bounds the union of the two <see cref="Area"/> structures.</returns>
        public static Area Union(Area a, Area b)
        {
            Rectangle result = Rectangle.Union(a.rectangle, b.rectangle);
            return new Area(result);
        }

        /// <summary>
        /// Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="pos">Amount to offset the location.</param>
        public void Offset(Cords pos) => rectangle.Offset(pos.Point);

        /// <summary>
        /// Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="x">The horizontal offset.</param>
        /// <param name="y">The vertical offset.</param>
        public void Offset(int x, int y) => rectangle.Offset(x, y);

#if HAS_DRAWING
        /// <summary>
        /// Creates a <see cref='Rectangle'/> with the specified <see cref='Area'/>.
        /// </summary>
        /// <param name="rect">The <see cref='Area'/> to convert.</param>
        /// <returns>The <see cref='Rectangle'/> that results from the conversion.</returns>
        public static implicit operator Rectangle(Area rect) => rect.rectangle;

        /// <summary>
        /// Creates a <see cref='Area'/> with the specified <see cref='Rectangle'/>.
        /// </summary>
        /// <param name="rect">The <see cref='Rectangle'/> to convert.</param>
        /// <returns>The <see cref='Area'/> that results from the conversion.</returns>
        public static implicit operator Area(Rectangle rect) => new(rect);
#else
        /// <inheritdoc/>
        readonly bool IEquatable<Rectangle>.Equals(Rectangle other) => Equals(other);
#endif

        /// <summary>
        /// Converts the attributes of this <see cref='Area'/> to a human readable string.
        /// </summary>
        /// <returns>A string that contains the position, width, and height of this <see cref="Area"/> structure ¾
        /// for example, <c>{X=20, Y=20, Width=100, Height=50}</c>.</returns>
        public override readonly string ToString() => rectangle.ToString();
    }
}
