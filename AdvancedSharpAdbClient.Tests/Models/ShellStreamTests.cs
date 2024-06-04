using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    public class ShellStreamTests
    {
        [Fact]
        public void ConstructorNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => new ShellStream(null, false));

        [Fact]
        public void ConstructorWriteOnlyTest()
        {
            string temp = Path.GetTempFileName();

            try
            {
                using FileStream stream = File.OpenWrite(temp);
                _ = Assert.Throws<ArgumentOutOfRangeException>(() => new ShellStream(stream, false));
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [Fact]
        public void ConstructorTest()
        {
            using MemoryStream stream = new();
            using ShellStream shellStream = new(stream, false);
            Assert.Equal(stream, shellStream.Inner);
            Assert.Equal(stream.CanRead, shellStream.CanRead);
            Assert.Equal(stream.CanSeek, shellStream.CanSeek);
            Assert.Equal(stream.CanWrite, shellStream.CanWrite);
        }

        [Fact]
        public void CRLFAtStartTest()
        {
            using MemoryStream stream = GetStream("\r\nHello, World!");
            using ShellStream shellStream = new(stream, false);
            using StreamReader reader = new(shellStream);
            Assert.Equal('\n', shellStream.ReadByte());

            stream.Position = 0;
            byte[] buffer = new byte[2];
            int read = shellStream.Read(buffer.AsSpan(0, 2));
            Assert.Equal(2, read);
            Assert.Equal((byte)'\n', buffer[0]);
            Assert.Equal((byte)'H', buffer[1]);

            stream.Position = 0;
            Assert.Equal("\nHello, World!", reader.ReadToEnd());
        }

        [Fact]
        public void MultipleCRLFInStringTest()
        {
            using MemoryStream stream = GetStream("\r\n1\r\n2\r\n3\r\n4\r\n5");
            using ShellStream shellStream = new(stream, false);
            using StreamReader reader = new(shellStream);
            Assert.Equal('\n', shellStream.ReadByte());

            stream.Position = 0;
            byte[] buffer = new byte[100];
            int read = shellStream.Read(buffer.AsSpan(0, 100));

            string actual = Encoding.ASCII.GetString(buffer, 0, read);
            Assert.Equal("\n1\n2\n3\n4\n5", actual);
            Assert.Equal(10, read);

            for (int i = 10; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void PendingByteTest()
        {
            using MemoryStream stream = GetStream("\r\nH\ra");
            using ShellStream shellStream = new(stream, false);
            byte[] buffer = new byte[1];
            int read = shellStream.Read(buffer.AsSpan(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'\n', buffer[0]);

            read = shellStream.Read(buffer.AsSpan(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'H', buffer[0]);

            read = shellStream.Read(buffer.AsSpan(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'\r', buffer[0]);

            read = shellStream.Read(buffer.AsSpan(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'a', buffer[0]);
        }

        [Fact]
        public async Task CRLFAtStartAsyncTest()
        {
            await using MemoryStream stream = GetStream("\r\nHello, World!");
            await using ShellStream shellStream = new(stream, false);
            using StreamReader reader = new(shellStream);
            Assert.Equal('\n', shellStream.ReadByte());

            stream.Position = 0;
            byte[] buffer = new byte[2];
            int read = await shellStream.ReadAsync(buffer.AsMemory(0, 2));
            Assert.Equal(2, read);
            Assert.Equal((byte)'\n', buffer[0]);
            Assert.Equal((byte)'H', buffer[1]);

            stream.Position = 0;
            Assert.Equal("\nHello, World!", reader.ReadToEnd());
        }

        [Fact]
        public async Task MultipleCRLFInStringAsyncTest()
        {
            await using MemoryStream stream = GetStream("\r\n1\r\n2\r\n3\r\n4\r\n5");
            await using ShellStream shellStream = new(stream, false);
            using StreamReader reader = new(shellStream);
            Assert.Equal('\n', shellStream.ReadByte());

            stream.Position = 0;
            byte[] buffer = new byte[100];
            int read = await shellStream.ReadAsync(buffer.AsMemory(0, 100));

            string actual = Encoding.ASCII.GetString(buffer, 0, read);
            Assert.Equal("\n1\n2\n3\n4\n5", actual);
            Assert.Equal(10, read);

            for (int i = 10; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public async Task PendingByteAsyncTest()
        {
            await using MemoryStream stream = GetStream("\r\nH\ra");
            await using ShellStream shellStream = new(stream, false);
            byte[] buffer = new byte[1];
            int read = await shellStream.ReadAsync(buffer.AsMemory(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'\n', buffer[0]);

            read = await shellStream.ReadAsync(buffer.AsMemory(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'H', buffer[0]);

            read = await shellStream.ReadAsync(buffer.AsMemory(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'\r', buffer[0]);

            read = await shellStream.ReadAsync(buffer.AsMemory(0, 1));
            Assert.Equal(1, read);
            Assert.Equal((byte)'a', buffer[0]);
        }

        private static MemoryStream GetStream(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            return new MemoryStream(data);
        }
    }
}
