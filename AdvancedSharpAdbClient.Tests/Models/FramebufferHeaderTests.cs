using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="FramebufferHeader"/> struct.
    /// </summary>
    public class FramebufferHeaderTests
    {
        [Fact]
        public void ReadFramebufferTest()
        {
            byte[] data = File.ReadAllBytes("Assets/FramebufferHeader.V1.bin");

            FramebufferHeader header = [.. data];

            Assert.Equal(8u, header.Alpha.Length);
            Assert.Equal(24u, header.Alpha.Offset);
            Assert.Equal(8u, header.Green.Length);
            Assert.Equal(8u, header.Green.Offset);
            Assert.Equal(8u, header.Red.Length);
            Assert.Equal(0u, header.Red.Offset);
            Assert.Equal(8u, header.Blue.Length);
            Assert.Equal(16u, header.Blue.Offset);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(14745600u, header.Size);
            Assert.Equal(2560u, header.Height);
            Assert.Equal(1440u, header.Width);
            Assert.Equal(1u, header.Version);
            Assert.Equal(0u, header.ColorSpace);

            for (int i = 0; i < header.Count; i++)
            {
                Assert.Equal(data[i], header[i]);
            }

            Assert.Equal(data.AsSpan(), [.. header]);
        }

        [Fact]
        public void ReadFramebufferV2Test()
        {
            byte[] data = File.ReadAllBytes("Assets/FramebufferHeader.V2.bin");

            FramebufferHeader header = [.. data];

            Assert.Equal(8u, header.Alpha.Length);
            Assert.Equal(24u, header.Alpha.Offset);
            Assert.Equal(8u, header.Green.Length);
            Assert.Equal(8u, header.Green.Offset);
            Assert.Equal(8u, header.Red.Length);
            Assert.Equal(0u, header.Red.Offset);
            Assert.Equal(8u, header.Blue.Length);
            Assert.Equal(16u, header.Blue.Offset);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(8294400u, header.Size);
            Assert.Equal(1920u, header.Height);
            Assert.Equal(1080u, header.Width);
            Assert.Equal(2u, header.Version);
            Assert.Equal(0u, header.ColorSpace);

            for (int i = 0; i < header.Count; i++)
            {
                Assert.Equal(data[i], header[i]);
            }

            Assert.Equal(data.AsSpan(), [.. header]);
        }

#if WINDOWS
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
    }
}
