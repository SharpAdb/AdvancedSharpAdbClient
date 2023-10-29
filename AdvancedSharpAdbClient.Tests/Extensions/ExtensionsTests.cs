using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="Extensions"/> class.
    /// </summary>
    public class ExtensionsTests
    {
        [Fact]
        public async void TaskToArrayTest()
        {
            int[] array = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
            Task<IEnumerable<int>> arrayTask = Extensions.Delay(10).ContinueWith(_ => array.Select(x => x));
            IEnumerable<Task<int>> taskArray = array.Select(x => Extensions.Delay(x).ContinueWith(_ => x));
            Assert.Equal(array, await taskArray.ToArrayAsync());
            Assert.Equal(array, await arrayTask.ToArrayAsync());
        }
    }
}
