using System;
using Xunit;

namespace AdvancedSharpAdbClient.DeviceCommands.Tests
{
    /// <summary>
    /// Tests the <see cref="LinuxPath"/> class.
    /// </summary>
    public class LinuxPathTests
    {
        [Fact]
        public void CheckInvalidPathCharsNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => LinuxPath.CheckInvalidPathChars(null));

        [Fact]
        public void CheckInvalidPathCharsTest() =>
            // Should not throw an exception.
            LinuxPath.CheckInvalidPathChars("/var/test");

        [Theory]
        [InlineData("/var/test > out")]
        [InlineData("\t/var/test")]
        public void CheckInvalidPathCharsThrowTest(string path) =>
            // Should throw an exception.
            _ = Assert.Throws<ArgumentException>(() => LinuxPath.CheckInvalidPathChars(path));

        [Theory]
        [InlineData(null)]
        [InlineData("/test", "hi", null)]
        public void CombineNullTest(params string[] paths) =>
            _ = Assert.Throws<ArgumentNullException>(() => LinuxPath.Combine(paths));

        [Fact]
        public void CombineTest()
        {
            string result = LinuxPath.Combine("/system", "busybox");
            Assert.Equal("/system/busybox", result);

            result = LinuxPath.Combine("/system/", "busybox");
            Assert.Equal("/system/busybox", result);

            result = LinuxPath.Combine("/system/xbin", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system/xbin/", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system//xbin", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system", "xbin", "busybox");
            Assert.Equal("/system/xbin/busybox", result);

            result = LinuxPath.Combine("/system", "xbin", "really", "long", "path", "to", "nothing");
            Assert.Equal("/system/xbin/really/long/path/to/nothing", result);
        }

        [Fact]
        public void CombineCurrentDirTest()
        {
            string result = LinuxPath.Combine(".", "Test.txt");
            Assert.Equal("./Test.txt", result);
        }

        [Fact]
        public void CombineDotDirTest()
        {
            string result = LinuxPath.Combine("Test.Test", "Test.txt");
            Assert.Equal("./Test.Test/Test.txt", result);

            result = LinuxPath.Combine("Test/Test.Test", "Test.txt");
            Assert.Equal("./Test/Test.Test/Test.txt", result);

            result = LinuxPath.Combine("/Test/Test.Test", "Test.txt");
            Assert.Equal("/Test/Test.Test/Test.txt", result);

            result = LinuxPath.Combine("/Test Test/Test.Test", "Test.txt");
            Assert.Equal("/Test Test/Test.Test/Test.txt", result);
        }

        [Fact]
        public void GetDirectoryNameTest()
        {
            string result = LinuxPath.GetDirectoryName("/system/busybox");
            Assert.Equal("/system/", result);

            result = LinuxPath.GetDirectoryName("/");
            Assert.Equal("/", result);

            result = LinuxPath.GetDirectoryName("/system/xbin/");
            Assert.Equal("/system/xbin/", result);

            result = LinuxPath.GetDirectoryName("echo");
            Assert.Equal("./", result);

            result = LinuxPath.GetDirectoryName(null);
            Assert.Null(result);
        }

        [Fact]
        public void GetFileNameTest()
        {
            string result = LinuxPath.GetFileName("/system/busybox");
            Assert.Equal("busybox", result);

            result = LinuxPath.GetFileName("/");
            Assert.Equal("", result);

            result = LinuxPath.GetFileName("/system/xbin/");
            Assert.Equal("", result);

            result = LinuxPath.GetFileName("/system/xbin/file.ext");
            Assert.Equal("file.ext", result);

            result = LinuxPath.GetFileName(null);
            Assert.Null(result);
        }

        [Fact]
        public void IsPathRootedTest()
        {
            Assert.True(LinuxPath.IsPathRooted("/system/busybox"));
            Assert.True(LinuxPath.IsPathRooted("/system/xbin/"));
            Assert.False(LinuxPath.IsPathRooted("system/xbin/"));
        }
    }
}
