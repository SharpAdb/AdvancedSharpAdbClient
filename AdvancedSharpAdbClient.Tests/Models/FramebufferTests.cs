using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="Framebuffer"/> class.
    /// </summary>
    public class FramebufferTests
    {
        [Fact]
        public void ConstructorNullTest()
        {
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => new Framebuffer(default, (AdbClient)null));
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => new Framebuffer(default, (EndPoint)null));
            _ = Assert.Throws<ArgumentNullException>(() => new Framebuffer(new DeviceData { Serial = "169.254.109.177:5555" }, (AdbClient)null));
            _ = Assert.Throws<ArgumentNullException>(() => new Framebuffer(new DeviceData { Serial = "169.254.109.177:5555" }, (EndPoint)null));
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => new Framebuffer(default, new AdbClient()));
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => new Framebuffer(default, AdbClient.DefaultEndPoint));
        }

        [Fact]
        public void RefreshTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            DummyAdbSocket socket = new();

            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);

            socket.Requests.Add("host:transport:169.254.109.177:5555");
            socket.Requests.Add("framebuffer:");

            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/FramebufferHeader.bin"));
            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/Framebuffer.bin"));

            using Framebuffer framebuffer = new(device, endPoint => socket);

            framebuffer.Refresh();

            Assert.NotNull(framebuffer);
            Assert.Equal(device, framebuffer.Device);
            Assert.Equal(16, framebuffer.Data.Length);

            FramebufferHeader header = framebuffer.Header;

            Assert.Equal(8u, header.Alpha.Length);
            Assert.Equal(24u, header.Alpha.Offset);
            Assert.Equal(8u, header.Green.Length);
            Assert.Equal(8u, header.Green.Offset);
            Assert.Equal(8u, header.Red.Length);
            Assert.Equal(0u, header.Red.Offset);
            Assert.Equal(8u, header.Blue.Length);
            Assert.Equal(16u, header.Blue.Offset);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(4u, header.Size);
            Assert.Equal(1u, header.Height);
            Assert.Equal(1u, header.Width);
            Assert.Equal(1u, header.Version);
            Assert.Equal(0u, header.ColorSpace);

#if WINDOWS
            if (!OperatingSystem.IsWindows()) { return; }

            using Bitmap image = (Bitmap)framebuffer;
            Assert.NotNull(image);
            Assert.Equal(PixelFormat.Format32bppArgb, image.PixelFormat);

            Assert.Equal(1, image.Width);
            Assert.Equal(1, image.Height);

            Color pixel = image.GetPixel(0, 0);
            Assert.Equal(0x35, pixel.R);
            Assert.Equal(0x4a, pixel.G);
            Assert.Equal(0x4c, pixel.B);
            Assert.Equal(0xff, pixel.A);
#endif
        }

        [Fact]
        public async Task RefreshAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            DummyAdbSocket socket = new();

            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);

            socket.Requests.Add("host:transport:169.254.109.177:5555");
            socket.Requests.Add("framebuffer:");

            socket.SyncDataReceived.Enqueue(await File.ReadAllBytesAsync("Assets/FramebufferHeader.bin"));
            socket.SyncDataReceived.Enqueue(await File.ReadAllBytesAsync("Assets/Framebuffer.bin"));

            using Framebuffer framebuffer = new(device, endPoint => socket);

            await framebuffer.RefreshAsync();

            Assert.NotNull(framebuffer);
            Assert.Equal(device, framebuffer.Device);
            Assert.Equal(16, framebuffer.Data.Length);

            FramebufferHeader header = framebuffer.Header;

            Assert.Equal(8u, header.Alpha.Length);
            Assert.Equal(24u, header.Alpha.Offset);
            Assert.Equal(8u, header.Green.Length);
            Assert.Equal(8u, header.Green.Offset);
            Assert.Equal(8u, header.Red.Length);
            Assert.Equal(0u, header.Red.Offset);
            Assert.Equal(8u, header.Blue.Length);
            Assert.Equal(16u, header.Blue.Offset);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(4u, header.Size);
            Assert.Equal(1u, header.Height);
            Assert.Equal(1u, header.Width);
            Assert.Equal(1u, header.Version);
            Assert.Equal(0u, header.ColorSpace);

#if WINDOWS
            if (!OperatingSystem.IsWindows()) { return; }

            using Bitmap image = (Bitmap)framebuffer;
            Assert.NotNull(image);
            Assert.Equal(PixelFormat.Format32bppArgb, image.PixelFormat);

            Assert.Equal(1, image.Width);
            Assert.Equal(1, image.Height);

            Color pixel = image.GetPixel(0, 0);
            Assert.Equal(0x35, pixel.R);
            Assert.Equal(0x4a, pixel.G);
            Assert.Equal(0x4c, pixel.B);
            Assert.Equal(0xff, pixel.A);
#endif
        }
    }
}
