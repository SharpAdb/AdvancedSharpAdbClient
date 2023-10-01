using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="Extensions"/> class.
    /// </summary>
    public class ExtensionsTests
    {
        /// <summary>
        /// Tests the <see cref="Extensions.AddRange{TSource}(ICollection{TSource}, IEnumerable{TSource})"/> method.
        /// </summary>
        [Fact]
        public void AddRangeTest()
        {
            int[] numbs = [6, 7, 8, 9, 10];

            List<int> list = [1, 2, 3, 4, 5];
            list.AddRange(numbs);
            Assert.Equal(10, list.Count);
            Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], list);

            HashSet<int> hashSet = [1, 2, 3, 4, 5];
            hashSet.AddRange(numbs);
            Assert.Equal(10, hashSet.Count);
            Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], hashSet);

            Collection<int> collection = [1, 2, 3, 4, 5];
            collection.AddRange(numbs);
            Assert.Equal(10, collection.Count);
            Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], collection);
        }

        /// <summary>
        /// Tests the <see cref="Extensions.TryParse{TEnum}(string, bool, out TEnum)"/> method.
        /// </summary>
        [Fact]
        public void TryParseTest()
        {
            Assert.True(Extensions.TryParse("BootLoader", false, out DeviceState result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.True(Extensions.TryParse("Bootloader", true, out result));
            Assert.Equal(DeviceState.BootLoader, result);
            Assert.False(Extensions.TryParse<DeviceState>("Bootloader", false, out _));
            Assert.False(Extensions.TryParse<DeviceState>("Reset", true, out _));
        }

        /// <summary>
        /// Tests the <see cref="Extensions.IsNullOrWhiteSpace(string)"/> method.
        /// </summary>
        [Fact]
        public void IsNullOrWhiteSpaceTest()
        {
            Assert.True(" ".IsNullOrWhiteSpace());
            Assert.False(" test ".IsNullOrWhiteSpace());
        }

        /// <summary>
        /// Tests the <see cref="Extensions.Join(string, IEnumerable{string})"/> method.
        /// </summary>
        [Fact]
        public void JoinTest() =>
            Assert.Equal("Hello World!", Extensions.Join(" ", ["Hello", "World!"]));
    }
}
