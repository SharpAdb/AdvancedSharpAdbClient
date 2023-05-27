using AdvancedSharpAdbClient.Extensions;
using Xunit;

namespace StringExtensions.Tests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("Hello", true)]
        public void IsNotNullOrEmpty_ShouldReturnCorrectResult(string value, bool expected)
        {
            // Arrange
            // Nothing to arrange

            // Act
            var result = value.IsNotNullOrEmpty();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("Hello", true)]
        [InlineData(" Hello ", true)]
        public void IsNotNullOrWhiteSpace_ShouldReturnCorrectResult(string value, bool expected)
        {
            // Arrange
            // Nothing to arrange

            // Act
            var result = value.IsNotNullOrWhiteSpace();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
