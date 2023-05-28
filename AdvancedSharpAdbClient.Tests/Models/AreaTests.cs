using System;
using System.Drawing;
using System.Globalization;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="Area"/> class.
    /// </summary>
    public class AreaTests
    {
        [Fact]
        public void DefaultConstructorTest()
        {
            Assert.Equal(Area.Empty, new Area());
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue, int.MaxValue, int.MinValue)]
        [InlineData(int.MaxValue, 0, int.MinValue, 0)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, int.MinValue, 0, int.MaxValue)]
        public void NonDefaultConstructorTest(int x, int y, int width, int height)
        {
            Area rect1 = new(x, y, width, height);
            Area rect2 = new(new Cords(x, y), new Size(width, height));

            Assert.Equal(rect1, rect2);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue, int.MaxValue, int.MinValue)]
        [InlineData(int.MaxValue, 0, int.MinValue, 0)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, int.MinValue, 0, int.MaxValue)]
        public void FromLTRBTest(int left, int top, int right, int bottom)
        {
            Area rect1 = new(left, top, unchecked(right - left), unchecked(bottom - top));
            Area rect2 = Area.FromLTRB(left, top, right, bottom);

            Assert.Equal(rect1, rect2);
        }

        [Fact]
        public void EmptyTest()
        {
            Assert.True(Area.Empty.IsEmpty);
            Assert.True(new Area(0, 0, 0, 0).IsEmpty);
            Assert.True(new Area().IsEmpty);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue, int.MaxValue, int.MinValue)]
        [InlineData(int.MaxValue, 0, int.MinValue, 0)]
        [InlineData(int.MinValue, int.MaxValue, int.MinValue, int.MaxValue)]
        [InlineData(0, int.MinValue, 0, int.MaxValue)]
        public void NonEmptyTest(int x, int y, int width, int height)
        {
            Assert.False(new Area(x, y, width, height).IsEmpty);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue, int.MaxValue, int.MinValue)]
        [InlineData(int.MaxValue, 0, int.MinValue, 0)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, int.MinValue, 0, int.MaxValue)]
        [InlineData(int.MinValue, int.MaxValue, int.MinValue, int.MaxValue)]
        public void DimensionsTest(int x, int y, int width, int height)
        {
            Area rect = new(x, y, width, height);
            Assert.Equal(new Cords(x, y), rect.Location);
            Assert.Equal(new Size(width, height), rect.Size);

            Assert.Equal(x, rect.X);
            Assert.Equal(y, rect.Y);
            Assert.Equal(width, rect.Width);
            Assert.Equal(height, rect.Height);
            Assert.Equal(x, rect.Left);
            Assert.Equal(y, rect.Top);
            Assert.Equal(unchecked(x + width), rect.Right);
            Assert.Equal(unchecked(y + height), rect.Bottom);

            Cords p = new(width, height);
            Size s = new(x, y);
            rect.Location = p;
            rect.Size = s;

            Assert.Equal(p, rect.Location);
            Assert.Equal(s, rect.Size);

            Assert.Equal(width, rect.X);
            Assert.Equal(height, rect.Y);
            Assert.Equal(x, rect.Width);
            Assert.Equal(y, rect.Height);
            Assert.Equal(width, rect.Left);
            Assert.Equal(height, rect.Top);
            Assert.Equal(unchecked(x + width), rect.Right);
            Assert.Equal(unchecked(y + height), rect.Bottom);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(int.MaxValue, int.MinValue)]
        public static void LocationSetTest(int x, int y)
        {
            Cords Cords = new(x, y);
            Area rect = new(10, 10, 10, 10)
            {
                Location = Cords
            };
            Assert.Equal(Cords, rect.Location);
            Assert.Equal(Cords.X, rect.X);
            Assert.Equal(Cords.Y, rect.Y);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(int.MaxValue, int.MinValue)]
        public static void SizeSetTest(int x, int y)
        {
            Size size = new(x, y);
            Area rect = new(10, 10, 10, 10)
            {
                Size = size
            };
            Assert.Equal(size, rect.Size);
            Assert.Equal(size.Width, rect.Width);
            Assert.Equal(size.Height, rect.Height);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue, int.MaxValue, int.MinValue)]
        [InlineData(int.MaxValue, 0, int.MinValue, 0)]
        [InlineData(0, int.MinValue, 0, int.MaxValue)]
        [InlineData(int.MinValue, int.MaxValue, int.MinValue, int.MaxValue)]
        public void EqualityTest(int x, int y, int width, int height)
        {
            Area rect1 = new(x, y, width, height);
            Area rect2 = new(width / 2, height / 2, x, y);

            Assert.True(rect1 != rect2);
            Assert.False(rect1 == rect2);
            Assert.False(rect1.Equals(rect2));
            Assert.False(rect1.Equals((object)rect2));
        }

        [Fact]
        public static void EqualityTest_NotArea()
        {
            Area Area = new(0, 0, 0, 0);
            Assert.False(Area.Equals(null));
            Assert.False(Area.Equals(0));
            Assert.False(Area.Equals(new RectangleF(0, 0, 0, 0)));
        }

        [Fact]
        public static void GetHashCodeTest()
        {
            Area rect1 = new(10, 10, 10, 10);
            Area rect2 = new(10, 10, 10, 10);
            Assert.Equal(rect1.GetHashCode(), rect2.GetHashCode());
            Assert.NotEqual(rect1.GetHashCode(), new Area(20, 10, 10, 10).GetHashCode());
            Assert.NotEqual(rect1.GetHashCode(), new Area(10, 20, 10, 10).GetHashCode());
            Assert.NotEqual(rect1.GetHashCode(), new Area(10, 10, 20, 10).GetHashCode());
            Assert.NotEqual(rect1.GetHashCode(), new Area(10, 10, 10, 20).GetHashCode());
        }

        [Theory]
        [InlineData(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue)]
        [InlineData(float.MinValue, float.MaxValue, float.MinValue, float.MaxValue)]
        [InlineData(0, 0, 0, 0)]
        public void AreaFConversionTest(float x, float y, float width, float height)
        {
            RectangleF rect = new(x, y, width, height);
            Area rCeiling, rTruncate, rRound;

            unchecked
            {
                rCeiling = new Area((int)Math.Ceiling(x), (int)Math.Ceiling(y),
                    (int)Math.Ceiling(width), (int)Math.Ceiling(height));
                rTruncate = new Area((int)x, (int)y, (int)width, (int)height);
                rRound = new Area((int)Math.Round(x), (int)Math.Round(y),
                    (int)Math.Round(width), (int)Math.Round(height));
            }

            Assert.Equal(rCeiling, Area.Ceiling(rect));
            Assert.Equal(rTruncate, Area.Truncate(rect));
            Assert.Equal(rRound, Area.Round(rect));
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue)]
        [InlineData(0, int.MinValue, int.MaxValue, 0)]
        public void ContainsTest(int x, int y, int width, int height)
        {
            Area rect = new(unchecked(2 * x - width), unchecked(2 * y - height), width, height);
            Cords p = new(x, y);
            Area r = new(x, y, width / 2, height / 2);

            Assert.False(rect.Contains(x, y));
            Assert.False(rect.Contains(p));
            Assert.False(rect.Contains(r));
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue)]
        [InlineData(0, int.MinValue, int.MaxValue, 0)]
        public void InflateTest(int x, int y, int width, int height)
        {
            Area inflatedRect, rect = new(x, y, width, height);
            unchecked
            {
                inflatedRect = new Area(x - width, y - height, width + 2 * width, height + 2 * height);
            }

            Assert.Equal(inflatedRect, Area.Inflate(rect, width, height));

            rect.Inflate(width, height);
            Assert.Equal(inflatedRect, rect);

            Size s = new(x, y);
            unchecked
            {
                inflatedRect = new Area(rect.X - x, rect.Y - y, rect.Width + 2 * x, rect.Height + 2 * y);
            }

            rect.Inflate(s);
            Assert.Equal(inflatedRect, rect);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue)]
        [InlineData(0, int.MinValue, int.MaxValue, 0)]
        public void IntersectTest(int x, int y, int width, int height)
        {
            Area rect = new(x, y, width, height);
            Area expectedRect = Area.Intersect(rect, rect);
            rect.Intersect(rect);
            Assert.Equal(expectedRect, rect);
            Assert.False(rect.IntersectsWith(expectedRect));
        }

        [Fact]
        public static void Intersect_IntersectingAreas_Test()
        {
            Area rect1 = new(0, 0, 5, 5);
            Area rect2 = new(1, 1, 3, 3);
            Area expected = new(1, 1, 3, 3);

            Assert.Equal(expected, Area.Intersect(rect1, rect2));
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue)]
        [InlineData(int.MaxValue, 0, 0, int.MaxValue)]
        [InlineData(0, int.MinValue, int.MaxValue, 0)]
        public void UnionTest(int x, int y, int width, int height)
        {
            Area a = new(x, y, width, height);
            Area b = new(width, height, x, y);

            int x1 = Math.Min(a.X, b.X);
            int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Min(a.Y, b.Y);
            int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            Area expectedArea = new(x1, y1, x2 - x1, y2 - y1);

            Assert.Equal(expectedArea, Area.Union(a, b));
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(int.MaxValue, int.MinValue, int.MinValue, int.MaxValue)]
        [InlineData(int.MaxValue, 0, 0, int.MaxValue)]
        [InlineData(0, int.MinValue, int.MaxValue, 0)]
        public void OffsetTest(int x, int y, int width, int height)
        {
            Area r1 = new(x, y, width, height);
            Area expectedRect = new(x + width, y + height, width, height);
            Cords p = new(width, height);

            r1.Offset(p);
            Assert.Equal(expectedRect, r1);

            expectedRect.Offset(p);
            r1.Offset(width, height);
            Assert.Equal(expectedRect, r1);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(5, -5, 0, 1)]
        public void ToStringTest(int x, int y, int width, int height)
        {
            Area r = new(x, y, width, height);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "{{X={0},Y={1},Width={2},Height={3}}}", r.X, r.Y, r.Width, r.Height), r.ToString());
        }
    }
}
