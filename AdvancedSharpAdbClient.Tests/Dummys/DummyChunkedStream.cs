using System;
using System.IO;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// A forward-only stream that returns its content in the given chunk sizes, like a pipe or network stream may.
    /// </summary>
    internal class DummyChunkedStream(byte[] content, params int[] chunks) : Stream
    {
        private int position;
        private int chunkIndex;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int chunk = chunkIndex < chunks.Length ? chunks[chunkIndex++] : count;
            int read = Math.Min(Math.Min(chunk, count), content.Length - position);
            Array.Copy(content, position, buffer, offset, read);
            position += read;
            return read;
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
