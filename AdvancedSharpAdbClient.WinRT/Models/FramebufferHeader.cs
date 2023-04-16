// <copyright file="FramebufferHeader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Runtime.InteropServices.WindowsRuntime;

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Whenever the <c>framebuffer:</c> service is invoked, the adb server responds with the contents
    /// of the framebuffer, prefixed with a <see cref="FramebufferHeader"/> object that contains more
    /// information about the framebuffer.
    /// </summary>
    public sealed class FramebufferHeader
    {
        internal AdvancedSharpAdbClient.FramebufferHeader framebufferHeader;

        /// <summary>
        /// Gets or sets the version of the framebuffer structure.
        /// </summary>
        public uint Version
        {
            get => framebufferHeader.Version;
            set => framebufferHeader.Version = value;
        }

        /// <summary>
        /// Gets or sets the number of bytes per pixel. Usual values include 32 or 24.
        /// </summary>
        public uint Bpp
        {
            get => framebufferHeader.Bpp;
            set => framebufferHeader.Bpp = value;
        }

        /// <summary>
        /// Gets or sets the color space. Only available starting with <see cref="Version"/> 2.
        /// </summary>
        public uint ColorSpace
        {
            get => framebufferHeader.ColorSpace;
            set => framebufferHeader.ColorSpace = value;
        }

        /// <summary>
        /// Gets or sets the total size, in bits, of the framebuffer.
        /// </summary>
        public uint Size
        {
            get => framebufferHeader.Size;
            set => framebufferHeader.Size = value;
        }
        /// <summary>
        /// Gets or sets the width, in pixels, of the framebuffer.
        /// </summary>
        public uint Width
        {
            get => framebufferHeader.Width;
            set => framebufferHeader.Width = value;
        }

        /// <summary>
        /// Gets or sets the height, in pixels, of the framebuffer.
        /// </summary>
        public uint Height
        {
            get => framebufferHeader.Height;
            set => framebufferHeader.Height = value;
        }

        /// <summary>
        /// Gets or sets information about the red color channel.
        /// </summary>
        public ColorData Red
        {
            get => ColorData.GetColorData(framebufferHeader.Red);
            set => framebufferHeader.Red = value.colorData;
        }

        /// <summary>
        /// Gets or sets information about the blue color channel.
        /// </summary>
        public ColorData Blue
        {
            get => ColorData.GetColorData(framebufferHeader.Blue);
            set => framebufferHeader.Blue = value.colorData;
        }

        /// <summary>
        /// Gets or sets information about the green color channel.
        /// </summary>
        public ColorData Green
        {
            get => ColorData.GetColorData(framebufferHeader.Green);
            set => framebufferHeader.Green = value.colorData;
        }

        /// <summary>
        /// Gets or sets information about the alpha channel.
        /// </summary>
        public ColorData Alpha
        {
            get => ColorData.GetColorData(framebufferHeader.Alpha);
            set => framebufferHeader.Alpha = value.colorData;
        }

        /// <summary>
        /// Creates a new <see cref="FramebufferHeader"/> object based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> structure.</param>
        /// <returns>A new <see cref="FramebufferHeader"/> object.</returns>
        public static FramebufferHeader Read([ReadOnlyArray] byte[] data) => GetFramebufferHeader(AdvancedSharpAdbClient.FramebufferHeader.Read(data));

        /// <summary>
        /// Initializes a new instance of the <see cref="FramebufferHeader"/> class.
        /// </summary>
        public FramebufferHeader() => framebufferHeader = new();

        internal FramebufferHeader(AdvancedSharpAdbClient.FramebufferHeader framebufferHeader) => this.framebufferHeader = framebufferHeader;

        internal static FramebufferHeader GetFramebufferHeader(AdvancedSharpAdbClient.FramebufferHeader framebufferHeader) => new(framebufferHeader);
    }
}
