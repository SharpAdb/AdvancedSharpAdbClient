using System;
using System.Drawing;
using System.Globalization;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="Cords"/> struct.
    /// </summary>
    public class CordsTests
    {
        [Fact]
        public void DefaultConstructorTest()
        {
            Assert.Equal(Cords.Empty, new Cords());
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        public void NonDefaultConstructorTest(int x, int y)
        {
            Cords p1 = new(x, y);
            Cords p2 = new Point(new Size(x, y));

            Assert.Equal(p1, p2);
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        public void SingleIntConstructorTest(int x)
        {
            Cords p1 = new(x);
            Cords p2 = new(unchecked((short)(x & 0xFFFF)), unchecked((short)((x >> 16) & 0xFFFF)));

            Assert.Equal(p1, p2);
        }

        [Fact]
        public void IsEmptyDefaultsTest()
        {
            Assert.True(Cords.Empty.IsEmpty);
            Assert.True(new Cords().IsEmpty);
            Assert.True(new Cords(0, 0).IsEmpty);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        public void IsEmptyRandomTest(int x, int y)
        {
            Assert.False(new Cords(x, y).IsEmpty);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        public void CoordinatesTest(int x, int y)
        {
            Cords p = new(x, y);
            Assert.Equal(x, p.X);
            Assert.Equal(y, p.Y);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        public void CordsFConversionTest(int x, int y)
        {
            PointF p = new Cords(x, y).Point;
            Assert.Equal(new PointF(x, y), p);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        public void SizeConversionTest(int x, int y)
        {
            Size sz = (Size)new Cords(x, y).Point;
            Assert.Equal(new Size(x, y), sz);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        public void ArithmeticTest(int x, int y)
        {
            Cords addExpected, subExpected, p = new(x, y);
            Size s = new(y, x);

            unchecked
            {
                addExpected = new Cords(x + y, y + x);
                subExpected = new Cords(x - y, y - x);
            }

            Assert.Equal<Cords>(addExpected, p.Point + s);
            Assert.Equal<Cords>(subExpected, p.Point - s);
            Assert.Equal<Cords>(addExpected, Point.Add(p, s));
            Assert.Equal<Cords>(subExpected, Point.Subtract(p, s));
        }

        [Theory]
        [InlineData(float.MaxValue, float.MinValue)]
        [InlineData(float.MinValue, float.MinValue)]
        [InlineData(float.MaxValue, float.MaxValue)]
        [InlineData(0, 0)]
        public void CordsFMathematicalTest(float x, float y)
        {
            PointF pf = new(x, y);
            Cords pCeiling, pTruncate, pRound;

            unchecked
            {
                pCeiling = new Cords((int)Math.Ceiling(x), (int)Math.Ceiling(y));
                pTruncate = new Cords((int)x, (int)y);
                pRound = new Cords((int)Math.Round(x), (int)Math.Round(y));
            }

            Assert.Equal<Cords>(pCeiling, Point.Ceiling(pf));
            Assert.Equal<Cords>(pRound, Point.Round(pf));
            Assert.Equal<Cords>(pTruncate, Point.Truncate(pf));
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        public void OffsetTest(int x, int y)
        {
            Cords p1 = new(x, y);
            Cords p2 = new(y, x);

            p1.Offset(p2);

            Assert.Equal(unchecked(p2.X + p2.Y), p1.X);
            Assert.Equal(p1.X, p1.Y);

            p2.Offset(x, y);
            Assert.Equal(p1, p2);
        }

        [Theory]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(0, 0)]
        public void EqualityTest(int x, int y)
        {
            Cords p1 = new(x, y);
            Cords p2 = new((x / 2) - 1, (y / 2) - 1);
            Cords p3 = new(x, y);

            Assert.True(p1 == p3);
            Assert.True(p1 != p2);
            Assert.True(p2 != p3);

            Assert.True(p1.Equals(p3));
            Assert.False(p1.Equals(p2));
            Assert.False(p2.Equals(p3));

            Assert.True(p1.Equals((object)p3));
            Assert.False(p1.Equals((object)p2));
            Assert.False(p2.Equals((object)p3));

            Assert.Equal(p1.GetHashCode(), p3.GetHashCode());
        }

        [Fact]
        public static void EqualityTest_NotCords()
        {
            Cords Cords = new(0, 0);
            Assert.False(Cords.Equals(null));
            Assert.False(Cords.Equals(0));
            Assert.False(Cords.Equals(new PointF(0, 0)));
        }

        [Fact]
        public static void GetHashCodeTest()
        {
            Cords Cords = new(10, 10);
            Assert.Equal(Cords.GetHashCode(), new Cords(10, 10).GetHashCode());
            Assert.NotEqual(Cords.GetHashCode(), new Cords(20, 10).GetHashCode());
            Assert.NotEqual(Cords.GetHashCode(), new Cords(10, 20).GetHashCode());
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, -2, 3, -4)]
        public void ConversionTest(int x, int y, int width, int height)
        {
            Area rect = new(x, y, width, height);
            RectangleF rectF = rect.Rectangle;
            Assert.Equal(x, rectF.X);
            Assert.Equal(y, rectF.Y);
            Assert.Equal(width, rectF.Width);
            Assert.Equal(height, rectF.Height);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(5, -5)]
        public void ToStringTest(int x, int y)
        {
            Cords p = new(x, y);
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "{{X={0},Y={1}}}", p.X, p.Y), p.ToString());
        }
    }
}
