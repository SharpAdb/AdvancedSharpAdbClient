using System;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="FramebufferHeader"/> struct.
    /// </summary>
    public class FramebufferHeaderTests
    {
        /// <summary>
        /// Tests the <see cref="FramebufferHeader(byte[])"/> method.
        /// </summary>
        [Fact]
        public void ReadFramebufferTest()
        {
            byte[] data = File.ReadAllBytes("Assets/FramebufferHeader.V1.bin");

            FramebufferHeader header = Read(default, data);

            Assert.Equal(1u, header.Version);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(0u, header.ColorSpace);
            Assert.Equal(14745600u, header.Size);
            Assert.Equal(1440u, header.Width);
            Assert.Equal(2560u, header.Height);

            ColorDataTest(header.Red, 0u, 8u, data.AsSpan(5 * sizeof(uint), ColorData.Size));
            ColorDataTest(header.Blue, 16u, 8u, data.AsSpan((5 * sizeof(uint)) + ColorData.Size, ColorData.Size));
            ColorDataTest(header.Green, 8u, 8u, data.AsSpan((5 * sizeof(uint)) + (2 * ColorData.Size), ColorData.Size));
            ColorDataTest(header.Alpha, 24u, 8u, data.AsSpan((5 * sizeof(uint)) + (3 * ColorData.Size), ColorData.Size));

            Assert.Equal(FramebufferHeader.MinLength, header.Count);
            for (int i = 0; i < header.Count; i++)
            {
                Assert.Equal(data[i], header[i]);
            }

            Assert.Equal(data.AsSpan(), [.. header]);
        }

        /// <summary>
        /// Tests the <see cref="FramebufferHeader(ReadOnlySpan{byte})"/> method.
        /// </summary>
        [Fact]
        public void ReadFramebufferBySpanTest()
        {
            Span<byte> data = File.ReadAllBytes("Assets/FramebufferHeader.V1.bin");

            FramebufferHeader header = [.. data];

            Assert.Equal(1u, header.Version);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(0u, header.ColorSpace);
            Assert.Equal(14745600u, header.Size);
            Assert.Equal(1440u, header.Width);
            Assert.Equal(2560u, header.Height);

            ColorDataTest(header.Red, 0u, 8u, data.Slice(5 * sizeof(uint), ColorData.Size));
            ColorDataTest(header.Blue, 16u, 8u, data.Slice((5 * sizeof(uint)) + ColorData.Size, ColorData.Size));
            ColorDataTest(header.Green, 8u, 8u, data.Slice((5 * sizeof(uint)) + (2 * ColorData.Size), ColorData.Size));
            ColorDataTest(header.Alpha, 24u, 8u, data.Slice((5 * sizeof(uint)) + (3 * ColorData.Size), ColorData.Size));

            Assert.Equal(FramebufferHeader.MinLength, header.Count);
            for (int i = 0; i < header.Count; i++)
            {
                Assert.Equal(data[i], header[i]);
            }

            Assert.Equal(data, [.. header]);
        }

        /// <summary>
        /// Tests the <see cref="FramebufferHeader(byte[])"/> method.
        /// </summary>
        [Fact]
        public void ReadFramebufferV2Test()
        {
            byte[] data = File.ReadAllBytes("Assets/FramebufferHeader.V2.bin");

            FramebufferHeader header = [.. data];

            Assert.Equal(2u, header.Version);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(0u, header.ColorSpace);
            Assert.Equal(8294400u, header.Size);
            Assert.Equal(1080u, header.Width);
            Assert.Equal(1920u, header.Height);

            ColorDataTest(header.Red, 0u, 8u, data.AsSpan(6 * sizeof(uint), ColorData.Size));
            ColorDataTest(header.Blue, 16u, 8u, data.AsSpan((6 * sizeof(uint)) + ColorData.Size, ColorData.Size));
            ColorDataTest(header.Green, 8u, 8u, data.AsSpan((6 * sizeof(uint)) + (2 * ColorData.Size), ColorData.Size));
            ColorDataTest(header.Alpha, 24u, 8u, data.AsSpan((6 * sizeof(uint)) + (3 * ColorData.Size), ColorData.Size));

            Assert.Equal(FramebufferHeader.MaxLength, header.Count);
            for (int i = 0; i < header.Count; i++)
            {
                Assert.Equal(data[i], header[i]);
            }

            Assert.Equal(data.AsSpan(), [.. header]);
        }

        /// <summary>
        /// Tests the <see cref="FramebufferHeader(ReadOnlySpan{byte})"/> method.
        /// </summary>
        [Fact]
        public void ReadFramebufferV2BySpanTest()
        {
            Span<byte> data = File.ReadAllBytes("Assets/FramebufferHeader.V2.bin");

            FramebufferHeader header = [.. data];

            Assert.Equal(2u, header.Version);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(0u, header.ColorSpace);
            Assert.Equal(8294400u, header.Size);
            Assert.Equal(1080u, header.Width);
            Assert.Equal(1920u, header.Height);

            ColorDataTest(header.Red, 0u, 8u, data.Slice(6 * sizeof(uint), ColorData.Size));
            ColorDataTest(header.Blue, 16u, 8u, data.Slice((6 * sizeof(uint)) + ColorData.Size, ColorData.Size));
            ColorDataTest(header.Green, 8u, 8u, data.Slice((6 * sizeof(uint)) + (2 * ColorData.Size), ColorData.Size));
            ColorDataTest(header.Alpha, 24u, 8u, data.Slice((6 * sizeof(uint)) + (3 * ColorData.Size), ColorData.Size));

            Assert.Equal(FramebufferHeader.MaxLength, header.Count);
            for (int i = 0; i < header.Count; i++)
            {
                Assert.Equal(data[i], header[i]);
            }

            Assert.Equal(data, [.. header]);
        }

#if HAS_IMAGING
        [Fact]
        public void ToImageTest()
        {
            if (!OperatingSystem.IsWindows()) { return; }

            byte[] data = File.ReadAllBytes("Assets/FramebufferHeader.bin");
            FramebufferHeader header = FramebufferHeader.Read(data);
            byte[] framebuffer = File.ReadAllBytes("Assets/Framebuffer.bin");
            using Bitmap image = header.ToImage(framebuffer);
            Assert.NotNull(image);
            Assert.Equal(PixelFormat.Format32bppArgb, image.PixelFormat);

            Assert.Equal(1, image.Width);
            Assert.Equal(1, image.Height);

            Color pixel = image.GetPixel(0, 0);
            Assert.Equal(0x35, pixel.R);
            Assert.Equal(0x4a, pixel.G);
            Assert.Equal(0x4c, pixel.B);
            Assert.Equal(0xff, pixel.A);
        }

        [Fact]
        public void ToImageEmptyTest()
        {
            if (!OperatingSystem.IsWindows()) { return; }

            byte[] data = File.ReadAllBytes("Assets/FramebufferHeader.Empty.bin");
            FramebufferHeader header = FramebufferHeader.Read(data);

            byte[] framebuffer = [];

            Bitmap image = header.ToImage(framebuffer);
            Assert.Null(image);
        }
#endif

        private static void ColorDataTest(ColorData actual, uint offset, uint length, Span<byte> data)
        {
            Assert.Equal(offset, actual.Offset);
            Assert.Equal(length, actual.Length);
            for (int i = 0; i < ColorData.Size; i++)
            {
                Assert.Equal(data[i], actual[i]);
            }
            Assert.Equal(data, [.. actual]);
            Assert.Equal(data.ToArray(), actual.ToArray());
            Assert.Equal(data, actual.AsSpan());
            (uint o, uint l) = actual;
            Assert.Equal(offset, o);
            Assert.Equal(length, l);
        }

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Read")]
        private static extern FramebufferHeader Read(FramebufferHeader header, byte[] data);
    }
}
