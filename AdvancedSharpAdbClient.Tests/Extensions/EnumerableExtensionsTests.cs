using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
            EnumerableExtensions.AddRange(list, numbs);
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
        /// Tests the <see cref="AdvancedSharpAdbClient.Tests.EnumerableExtensions.ToArrayAsync{TSource}(IEnumerable{Task{TSource}})"/> method.
        /// </summary>
        [Fact]
        public async Task TaskToArrayTest()
        {
            int[] array = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
            Task<IEnumerable<int>> arrayTask = Task.Delay(10, TestContext.Current.CancellationToken).ContinueWith(_ => array.Select(x => x));
            IEnumerable<Task<int>> taskArray = array.Select(x => Task.Delay(x, TestContext.Current.CancellationToken).ContinueWith(_ => x));
            Assert.Equal(array, await taskArray.ToArrayAsync());
            Assert.Equal(array, await arrayTask.ToListAsync());
        }
    }
}
