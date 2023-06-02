// <copyright file="Framebuffer.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides access to the framebuffer (that is, a copy of the image being displayed on the device screen).
    /// </summary>
    public class Framebuffer : IDisposable
    {
        private readonly AdbClient client;
        private byte[] headerData;
        private bool headerInitialized;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Framebuffer"/> class.
        /// </summary>
        /// <param name="device">The device for which to fetch the frame buffer.</param>
        /// <param name="client">A <see cref="AdbClient"/> which manages the connection with adb.</param>
        public Framebuffer(DeviceData device, AdbClient client)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));

            this.client = client ?? throw new ArgumentNullException(nameof(client));

            // Initialize the headerData buffer
            var size = Marshal.SizeOf(default(FramebufferHeader));
            this.headerData = new byte[size];
        }

        /// <summary>
        /// Gets the device for which to fetch the frame buffer.
        /// </summary>
        public DeviceData Device { get; private set; }

        /// <summary>
        /// Gets the framebuffer header. The header contains information such as the width and height and the color encoding.
        /// This property is set after you call <see cref="RefreshAsync(CancellationToken)"/>.
        /// </summary>
        public FramebufferHeader Header { get; private set; }

        /// <summary>
        /// Gets the framebuffer data. You need to parse the <see cref="FramebufferHeader"/> to interpret this data (such as the color encoding).
        /// This property is set after you call <see cref="RefreshAsync(CancellationToken)"/>.
        /// </summary>
        public byte[] Data { get; private set; }

#if HAS_TASK
        /// <summary>
        /// Asynchronously refreshes the framebuffer: fetches the latest framebuffer data from the device. Access the <see cref="Header"/>
        /// and <see cref="Data"/> properties to get the updated framebuffer data.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();

            using IAdbSocket socket = Factories.AdbSocketFactory(client.EndPoint);
            // Select the target device
            socket.SetDevice(Device);

            // Send the framebuffer command
            socket.SendAdbRequest("framebuffer:");
            socket.ReadAdbResponse();

            // The result first is a FramebufferHeader object,
            await socket.ReadAsync(headerData, cancellationToken).ConfigureAwait(false);

            if (!headerInitialized)
            {
                Header = FramebufferHeader.Read(headerData);
                headerInitialized = true;
            }

            if (Data == null || Data.Length < Header.Size)
            {
                // Optimization on .NET Core: Use the BufferPool to rent buffers
                if (Data != null)
                {
                    ArrayPool<byte>.Shared.Return(Data, clearArray: false);
                }

                Data = ArrayPool<byte>.Shared.Rent((int)Header.Size);
            }

            // followed by the actual framebuffer content
            _ = await socket.ReadAsync(Data, (int)Header.Size, cancellationToken).ConfigureAwait(false);
        }
#endif

#if HAS_DRAWING
        /// <summary>
        /// Converts the framebuffer data to a <see cref="Image"/>.
        /// </summary>
        /// <returns>An <see cref="Image"/> which represents the framebuffer data.</returns>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public Image ToImage()
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException("Call RefreshAsync first") : Header.ToImage(Data);
        }

        /// <inheritdoc/>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static explicit operator Image(Framebuffer value) => value.ToImage();
#endif

#if WINDOWS_UWP
        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        public Task<WriteableBitmap> ToBitmap()
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException("Call RefreshAsync first") : Header.ToBitmap(Data);
        }

        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="dispatcher">The target <see cref="CoreDispatcher"/> to invoke the code on.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        public Task<WriteableBitmap> ToBitmap(CoreDispatcher dispatcher)
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException("Call RefreshAsync first") : Header.ToBitmap(Data, dispatcher);
        }

        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="dispatcher">The target <see cref="DispatcherQueue"/> to invoke the code on.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        public Task<WriteableBitmap> ToBitmap(DispatcherQueue dispatcher)
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException("Call RefreshAsync first") : Header.ToBitmap(Data, dispatcher);
        }
#endif

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (Data != null)
                {
                    ArrayPool<byte>.Shared.Return(Data, clearArray: false);
                }

                headerData = null;
                headerInitialized = false;
                disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws an exception if this <see cref="Framebuffer"/> has been disposed.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (disposed)
            {
                ExceptionExtensions.ThrowIf(disposed, this);
            }
        }
    }
}
