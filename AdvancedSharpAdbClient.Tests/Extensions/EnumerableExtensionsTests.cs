using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace AdvancedSharpAdbClient.Polyfills.Tests
{
    /// <summary>
    /// Tests the <see cref="EnumerableExtensions"/> class.
    /// </summary>
    public class EnumerableExtensionsTests
    {
        /// <summary>
        /// Tests the <see cref="EnumerableExtensions.AddRange{TSource}(ICollection{TSource}, IEnumerable{TSource})"/> method.
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
    }
}
