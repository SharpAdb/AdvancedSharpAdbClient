using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Stores the location and size of a rectangular region.
    /// </summary>
    public struct Area : IEquatable<Area>
    {
        /// <summary>
        /// Represents a <see cref="Area"/> structure with its properties left uninitialized.
        /// </summary>
        public static readonly Area Empty;

        private int x; // Do not rename (binary serialization)
        private int y; // Do not rename (binary serialization)
        private int width; // Do not rename (binary serialization)
        private int height; // Do not rename (binary serialization)

        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> class with the specified location
        /// and size.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the area.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the area.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        public Area(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

#if HAS_DRAWING
        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> class with the specified rectangle.
        /// </summary>
        /// <param name="rectangle">A <see cref="System.Drawing.Rectangle"/> that represents the rectangular region.</param>
        public Area(System.Drawing.Rectangle rectangle)
        {
            x = rectangle.X;
            y = rectangle.Y;
            width = rectangle.Width;
            height = rectangle.Height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> class with the specified location and size.
        /// </summary>
        /// <param name="location">A <see cref="Cords"/> that represents the upper-left corner of the rectangular region.</param>
        /// <param name="size">A <see cref="System.Drawing.Size"/> that represents the width and height of the rectangular region.</param>
        public Area(Cords location, System.Drawing.Size size)
        {
            x = location.X;
            y = location.Y;
            width = size.Width;
            height = size.Height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> class with the specified location and size.
        /// </summary>
        /// <param name="location">A <see cref="System.Drawing.Point"/> that represents the upper-left corner of the rectangular region.</param>
        /// <param name="size">A <see cref="System.Drawing.Size"/> that represents the width and height of the rectangular region.</param>
        public Area(System.Drawing.Point location, System.Drawing.Size size)
        {
            x = location.X;
            y = location.Y;
            width = size.Width;
            height = size.Height;
        }
#endif

#if WINDOWS_UWP
#pragma warning disable CS0419 // cref 特性中有不明确的引用
        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> class with the specified rectangle.
        /// </summary>
        /// <param name="rectangle">A <see cref="Windows.Foundation.Rect"/> that represents the rectangular region.</param>
        public Area(Windows.Foundation.Rect rectangle)
        {
            x = unchecked((int)rectangle.X);
            y = unchecked((int)rectangle.Y);
            width = unchecked((int)rectangle.Width);
            height = unchecked((int)rectangle.Height);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> class with the specified location and size.
        /// </summary>
        /// <param name="location">A <see cref="Cords"/> that represents the upper-left corner of the rectangular region.</param>
        /// <param name="size">A <see cref="Windows.Foundation.Size"/> that represents the width and height of the rectangular region.</param>
        public Area(Cords location, Windows.Foundation.Size size)
        {
            x = location.X;
            y = location.Y;
            width = unchecked((int)size.Width);
            height = unchecked((int)size.Height);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='Area'/> class with the specified location and size.
        /// </summary>
        /// <param name="location">A <see cref="Windows.Foundation.Point"/> that represents the upper-left corner of the rectangular region.</param>
        /// <param name="size">A <see cref="Windows.Foundation.Size"/> that represents the width and height of the rectangular region.</param>
        public Area(Windows.Foundation.Point location, Windows.Foundation.Size size)
        {
            x = unchecked((int)location.X);
            y = unchecked((int)location.Y);
            width = unchecked((int)size.Width);
            height = unchecked((int)size.Height);
        }
#pragma warning restore CS0419 // cref 特性中有不明确的引用
#endif

        /// <summary>
        /// Creates a new <see cref='Area'/> with the specified location and size.
        /// </summary>
        /// <param name="left">The x-coordinate of the upper-left corner of this <see cref='Area'/> structure.</param>
        /// <param name="top">The y-coordinate of the upper-left corner of this <see cref='Area'/> structure.</param>
        /// <param name="right">The x-coordinate of the lower-right corner of this <see cref='Area'/> structure.</param>
        /// <param name="bottom">The y-coordinate of the lower-right corner of this <see cref='Area'/> structure.</param>
        /// <returns>The new <see cref="Area"/> that this method creates.</returns>
        public static Area FromLTRB(int left, int top, int right, int bottom) =>
            new(left, top, unchecked(right - left), unchecked(bottom - top));

        /// <summary>
        /// Gets or sets the coordinates of the upper-left corner of the rectangular region represented by this
        /// <see cref='Area'/>.
        /// </summary>
        public Cords Location
        {
            readonly get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the coordinates of the center of the rectangular region represented by this
        /// <see cref='Area'/>.
        /// </summary>
        public readonly Cords Center => unchecked(new(X + (Width / 2), Y + (Height / 2)));

#if HAS_DRAWING
        /// <summary>
        /// Gets or sets the size of this <see cref='Area'/>.
        /// </summary>
        public System.Drawing.Size Size
        {
            readonly get => new(Width, Height);
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }
#endif

#if !HAS_DRAWING && WINDOWS_UWP
        /// <summary>
        /// Gets or sets the size of this <see cref='Area'/>.
        /// </summary>
        public Windows.Foundation.Size Size
        {
            readonly get => new(Width, Height);
            set
            {
                Width = unchecked((int)value.Width);
                Height = unchecked((int)value.Height);
            }
        }
#endif

        /// <summary>
        /// Gets or sets the x-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public int X
        {
            readonly get => x;
            set => x = value;
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public int Y
        {
            readonly get => y;
            set => y = value;
        }

        /// <summary>
        /// Gets or sets the width of the rectangular region defined by this <see cref='Area'/>.
        /// </summary>
        public int Width
        {
            readonly get => width;
            set => width = value;
        }

        /// <summary>
        /// Gets or sets the width of the rectangular region defined by this <see cref='Area'/>.
        /// </summary>
        public int Height
        {
            readonly get => height;
            set => height = value;
        }

        /// <summary>
        /// Gets the x-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/> .
        /// </summary>
        public readonly int Left => X;

        /// <summary>
        /// Gets the y-coordinate of the upper-left corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public readonly int Top => Y;

        /// <summary>
        /// Gets the x-coordinate of the lower-right corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public readonly int Right => unchecked(X + Width);

        /// <summary>
        /// Gets the y-coordinate of the lower-right corner of the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        public readonly int Bottom => unchecked(Y + Height);

        /// <summary>
        /// Tests whether this <see cref='Area'/> has a <see cref='Area.Width'/>
        /// or a <see cref='Area.Height'/> of 0.
        /// </summary>
        public readonly bool IsEmpty => height == 0 && width == 0 && x == 0 && y == 0;

        /// <summary>
        /// Tests whether <paramref name="obj"/> is a <see cref='Area'/> with the same location
        /// and size of this Area.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if <paramref name="obj"/> is a <see cref="Area"/> structure
        /// and its <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties are equal to
        /// the corresponding properties of this <see cref="Area"/> structure; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object obj) => obj is Area area && Equals(area);

        /// <inheritdoc/>
        public readonly bool Equals(Area other) => this == other;

#if HAS_DRAWING
        /// <summary>
        /// Creates a <see cref='System.Drawing.Rectangle'/> with the specified <see cref='Area'/>.
        /// </summary>
        /// <param name="rect">The <see cref='Area'/> to convert.</param>
        /// <returns>The <see cref='System.Drawing.Rectangle'/> that results from the conversion.</returns>
        public static implicit operator System.Drawing.Rectangle(Area rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Creates a <see cref='System.Drawing.RectangleF'/> with the specified <see cref='Area'/>.
        /// </summary>
        /// <param name="rect">The <see cref='Area'/> to convert.</param>
        /// <returns>The <see cref='System.Drawing.RectangleF'/> that results from the conversion.</returns>
        public static implicit operator System.Drawing.RectangleF(Area rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Creates a <see cref='Area'/> with the specified <see cref='System.Drawing.Rectangle'/>.
        /// </summary>
        /// <param name="rect">The <see cref='System.Drawing.Rectangle'/> to convert.</param>
        /// <returns>The <see cref='Area'/> that results from the conversion.</returns>
        public static implicit operator Area(System.Drawing.Rectangle rect) => new(rect);
#endif

#if WINDOWS_UWP
#pragma warning disable CS0419 // cref 特性中有不明确的引用
        /// <summary>
        /// Creates a <see cref='Windows.Foundation.Rect'/> with the specified <see cref='Area'/>.
        /// </summary>
        /// <param name="rect">The <see cref='Area'/> to convert.</param>
        /// <returns>The <see cref='Windows.Foundation.Rect'/> that results from the conversion.</returns>
        public static implicit operator Windows.Foundation.Rect(Area rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Creates a <see cref='Area'/> with the specified <see cref='Windows.Foundation.Rect'/>.
        /// </summary>
        /// <param name="rect">The <see cref='Windows.Foundation.Rect'/> to convert.</param>
        /// <returns>The <see cref='Area'/> that results from the conversion.</returns>
        public static implicit operator Area(Windows.Foundation.Rect rect) => new(rect);
#pragma warning restore CS0419 // cref 特性中有不明确的引用
#endif

        /// <summary>
        /// Tests whether two <see cref='Area'/> objects have equal location and size.
        /// </summary>
        /// <param name="left">The Rectangle structure that is to the left of the equality operator.</param>
        /// <param name="right">The Rectangle structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="Area"/> structures have equal
        /// <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, and <see cref="Height"/> properties.</returns>
        public static bool operator ==(Area left, Area right) =>
            left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;

        /// <summary>
        /// Tests whether two <see cref='Area'/> objects differ in location or size.
        /// </summary>
        /// <param name="left">The Rectangle structure that is to the left of the inequality operator.</param>
        /// <param name="right">The Rectangle structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if any of the <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>
        /// properties of the two <see cref="Area"/> structures are unequal; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(Area left, Area right) => !(left == right);

#if HAS_DRAWING
        /// <summary>
        /// Converts a <see cref="System.Drawing.RectangleF"/> to a <see cref="Area"/> by performing a ceiling operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref="System.Drawing.RectangleF"/> structure to be converted.</param>
        public static Area Ceiling(System.Drawing.RectangleF value)
        {
            unchecked
            {
                return new Area(
                    (int)Math.Ceiling(value.X),
                    (int)Math.Ceiling(value.Y),
                    (int)Math.Ceiling(value.Width),
                    (int)Math.Ceiling(value.Height));
            }
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.RectangleF"/> to a <see cref="Area"/> by performing a truncate operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref="System.Drawing.RectangleF"/> structure to be converted.</param>
        public static Area Truncate(System.Drawing.RectangleF value)
        {
            unchecked
            {
                return new Area(
                    (int)value.X,
                    (int)value.Y,
                    (int)value.Width,
                    (int)value.Height);
            }
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.RectangleF"/> to a <see cref="Area"/> by performing a round operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref="System.Drawing.RectangleF"/> structure to be converted.</param>
        public static Area Round(System.Drawing.RectangleF value)
        {
            unchecked
            {
                return new Area(
                    (int)Math.Round(value.X),
                    (int)Math.Round(value.Y),
                    (int)Math.Round(value.Width),
                    (int)Math.Round(value.Height));
            }
        }
#endif

#if WINDOWS_UWP
#pragma warning disable CS0419 // cref 特性中有不明确的引用
        /// <summary>
        /// Converts a <see cref="Windows.Foundation.Rect"/> to a <see cref="Area"/> by performing a ceiling operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref="Windows.Foundation.Rect"/> structure to be converted.</param>
        public static Area Ceiling(Windows.Foundation.Rect value)
        {
            unchecked
            {
                return new Area(
                    (int)Math.Ceiling(value.X),
                    (int)Math.Ceiling(value.Y),
                    (int)Math.Ceiling(value.Width),
                    (int)Math.Ceiling(value.Height));
            }
        }

        /// <summary>
        /// Converts a <see cref="Windows.Foundation.Rect"/> to a <see cref="Area"/> by performing a truncate operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref="Windows.Foundation.Rect"/> structure to be converted.</param>
        public static Area Truncate(Windows.Foundation.Rect value)
        {
            unchecked
            {
                return new Area(
                    (int)value.X,
                    (int)value.Y,
                    (int)value.Width,
                    (int)value.Height);
            }
        }

        /// <summary>
        /// Converts a <see cref="Windows.Foundation.Rect"/> to a <see cref="Area"/> by performing a round operation on all the coordinates.
        /// </summary>
        /// <param name="value">The <see cref="Windows.Foundation.Rect"/> structure to be converted.</param>
        public static Area Round(Windows.Foundation.Rect value)
        {
            unchecked
            {
                return new Area(
                    (int)Math.Round(value.X),
                    (int)Math.Round(value.Y),
                    (int)Math.Round(value.Width),
                    (int)Math.Round(value.Height));
            }
        }
#pragma warning restore CS0419 // cref 特性中有不明确的引用
#endif

        /// <summary>
        /// Determines if the specified point is contained within the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to test.</param>
        /// <param name="y">The y-coordinate of the point to test.</param>
        /// <returns>This method returns <see langword="true"/> if the point defined by <paramref name="x"/> and <paramref name="y"/>
        /// is contained within this <see cref="Area"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(int x, int y) => X <= x && x < X + Width && Y <= y && y < Y + Height;

        /// <summary>
        /// Determines if the specified point is contained within the rectangular region defined by this
        /// <see cref='Area'/>.
        /// </summary>
        /// <param name="pt">The <see cref="Cords"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if the point represented by <paramref name="pt"/>
        /// is contained within this <see cref="Area"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(Cords pt) => Contains(pt.X, pt.Y);

        /// <summary>
        /// Determines if the rectangular region represented by <paramref name="rect"/> is entirely contained within the
        /// rectangular region represented by this <see cref='Area'/>.
        /// </summary>
        /// <param name="rect">The <see cref="Area"/> to test.</param>
        /// <returns>This method returns <see langword="true"/> if the rectangular region represented by <paramref name="rect"/>
        /// is entirely contained within this <see cref="Area"/> structure; otherwise <see langword="false"/>.</returns>
        public readonly bool Contains(Area rect) =>
            (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
            (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);

        /// <summary>
        /// Returns the hash code for this <see cref="Area"/> structure.
        /// </summary>
        /// <returns>An integer that represents the hash code for this rectangle.</returns>
        public override readonly int GetHashCode() =>
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            HashCode.Combine(X, Y, Width, Height);
#else
            X ^ Y ^ Width ^ Height;
#endif

        /// <summary>
        /// Inflates this <see cref='Area'/> by the specified amount.
        /// </summary>
        /// <param name="width">The amount to inflate this <see cref="Area"/> horizontally.</param>
        /// <param name="height">The amount to inflate this <see cref="Area"/> vertically.</param>
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

#if HAS_DRAWING
        /// <summary>
        /// Inflates this <see cref='Area'/> by the specified amount.
        /// </summary>
        /// <param name="size">The amount to inflate this rectangle.</param>
        public void Inflate(System.Drawing.Size size) => Inflate(size.Width, size.Height);
#endif

#if WINDOWS_UWP
        /// <summary>
        /// Inflates this <see cref='Area'/> by the specified amount.
        /// </summary>
        /// <param name="size">The amount to inflate this rectangle.</param>
        public void Inflate(Windows.Foundation.Size size) => Inflate(unchecked((int)size.Width), unchecked((int)size.Height));
#endif

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
        /// Creates a Area that represents the intersection between this Area and rect.
        /// </summary>
        /// <param name="rect">The <see cref="Area"/> with which to intersect.</param>
        public void Intersect(Area rect)
        {
            Area result = Intersect(rect, this);

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
        /// <returns>A <see cref="Area"/> that represents the intersection of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static Area Intersect(Area a, Area b)
        {
            int x1 = Math.Max(a.X, b.X);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Max(a.Y, b.Y);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            return x2 >= x1 && y2 >= y1 ? new Area(x1, y1, x2 - x1, y2 - y1) : Empty;
        }

        /// <summary>
        /// Determines if this rectangle intersects with rect.
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <returns>This method returns <see langword="true"/> if there is any intersection, otherwise <see langword="false"/>.</returns>
        public readonly bool IntersectsWith(Area rect) =>
            (rect.X < X + Width) && (X < rect.X + rect.Width) &&
            (rect.Y < Y + Height) && (Y < rect.Y + rect.Height);

        /// <summary>
        /// Creates a rectangle that represents the union between a and b.
        /// </summary>
        /// <param name="a">A rectangle to union.</param>
        /// <param name="b">A rectangle to union.</param>
        /// <returns>A <see cref="Area"/> structure that bounds the union of the two <see cref="Area"/> structures.</returns>
        public static Area Union(Area a, Area b)
        {
            int x1 = Math.Min(a.X, b.X);
            int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Min(a.Y, b.Y);
            int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new Area(x1, y1, x2 - x1, y2 - y1);
        }

        /// <summary>
        /// Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="pos">Amount to offset the location.</param>
        public void Offset(Cords pos) => Offset(pos.X, pos.Y);

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
        /// Converts the attributes of this <see cref='Area'/> to a human readable string.
        /// </summary>
        /// <returns>A string that contains the position, width, and height of this <see cref="Area"/> structure ¾
        /// for example, <c>{X=20, Y=20, Width=100, Height=50}</c>.</returns>
        public override readonly string ToString() => $"{{X={X},Y={Y},Width={Width},Height={Height}}}";
    }
}
