using System;
using Xunit;

namespace AdvancedSharpAdbClient.Polyfills.Tests
{
    /// <summary>
    /// Tests the <see cref="ExceptionExtensions"/> class.
    /// </summary>
    public class ExceptionExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("name")]
        public void ThrowIfNullTest(string paramName)
        {
            foreach (object o in new[] { new object(), string.Empty, "argument" })
            {
                ExceptionExtensions.ThrowIfNull(o);
                ExceptionExtensions.ThrowIfNull(o, nameof(paramName));
            }

            Assert.Equal(paramName, Assert.Throws<ArgumentNullException>(() => ExceptionExtensions.ThrowIfNull(null, paramName)).ParamName);
        }

        [Fact]
        public void ThrowIfGreaterThanTest()
        {
            Assert.Equal(1, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfGreaterThan(1, 0)).ActualValue);
            Assert.Equal(1u, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfGreaterThan<uint>(1, 0)).ActualValue);
            Assert.Equal(1.000000001, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfGreaterThan(1.000000001, 1)).ActualValue);
            Assert.Equal(1.00001f, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfGreaterThan(1.00001f, 1)).ActualValue);

            ExceptionExtensions.ThrowIfGreaterThan(1, 2);
        }

        [Fact]
        public static void ThrowIfLessThanTest()
        {
            Assert.Equal(0, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfLessThan(0, 1)).ActualValue);
            Assert.Equal(0u, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfLessThan<uint>(0, 1)).ActualValue);
            Assert.Equal(1d, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfLessThan(1, 1.000000001)).ActualValue);
            Assert.Equal(1f, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfLessThan(1, 1.00001f)).ActualValue);

            ExceptionExtensions.ThrowIfLessThan(2, 1);
        }

        [Fact]
        public static void ThrowIfNegativeTest()
        {
            Assert.Equal(-1d, Assert.Throws<ArgumentOutOfRangeException>(() => ExceptionExtensions.ThrowIfNegative(-1d)).ActualValue);

            ExceptionExtensions.ThrowIfNegative(0);
            ExceptionExtensions.ThrowIfNegative(1);
        }

        [Fact]
        public static void ThrowIfTest()
        {
            object obj = new();
            ObjectDisposedException ex = Assert.Throws<ObjectDisposedException>(
                () => ExceptionExtensions.ThrowIf(true, obj));

            Assert.Equal("System.Object", ex.ObjectName);
        }
    }
}
