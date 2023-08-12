// <copyright file="Framebuffer.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides access to the framebuffer (that is, a copy of the image being displayed on the device screen).
    /// </summary>
    public class Framebuffer : IDisposable
    {
        private byte[] headerData;
        private bool headerInitialized;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Framebuffer"/> class.
        /// </summary>
        /// <param name="device">The device for which to fetch the frame buffer.</param>
        /// <param name="endPoint">The <see cref="EndPoint"/> at which the adb server is listening.</param>
        public Framebuffer(DeviceData device, EndPoint endPoint)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));

            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));

            // Initialize the headerData buffer
#if NETCORE
            headerData = new byte[56];
#else
#if !NETFRAMEWORK || NET451_OR_GREATER
            int size = Marshal.SizeOf<FramebufferHeader>();
#else
            int size = Marshal.SizeOf(default(FramebufferHeader));
#endif
            headerData = new byte[size];
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Framebuffer"/> class.
        /// </summary>
        /// <param name="device">The device for which to fetch the frame buffer.</param>
        /// <param name="client">A <see cref="AdbClient"/> which manages the connection with adb.</param>
        public Framebuffer(DeviceData device, AdbClient client) : this(device, client?.EndPoint) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Framebuffer"/> class.
        /// </summary>
        /// <param name="device">The device for which to fetch the frame buffer.</param>
        public Framebuffer(DeviceData device) : this(device, AdbClient.DefaultEndPoint) { }

        /// <summary>
        /// Gets the device for which to fetch the frame buffer.
        /// </summary>
        public DeviceData Device { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.EndPoint"/> at which the adb server is listening.
        /// </summary>
        public EndPoint EndPoint { get; }

        /// <summary>
        /// Gets the framebuffer header. The header contains information such as the width and height and the color encoding.
        /// This property is set after you call <see cref="Refresh"/>.
        /// </summary>
        public FramebufferHeader Header { get; private set; }

        /// <summary>
        /// Gets the framebuffer data. You need to parse the <see cref="FramebufferHeader"/> to interpret this data (such as the color encoding).
        /// This property is set after you call <see cref="Refresh"/>.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Refreshes the framebuffer: fetches the latest framebuffer data from the device. Access the <see cref="Header"/>
        /// and <see cref="Data"/> properties to get the updated framebuffer data.
        /// </summary>
        /// <param name="reset">Refreshes the header of framebuffer when <see langword="true"/>.</param>
        public void Refresh(bool reset = false)
        {
            EnsureNotDisposed();

            using IAdbSocket socket = Factories.AdbSocketFactory(EndPoint);
            // Select the target device
            socket.SetDevice(Device);

            // Send the framebuffer command
            socket.SendAdbRequest("framebuffer:");
            socket.ReadAdbResponse();

            // The result first is a FramebufferHeader object,
            socket.Read(headerData);

            if (reset || !headerInitialized)
            {
                Header = FramebufferHeader.Read(headerData);
                headerInitialized = true;
            }

            if (Data == null || Data.Length < Header.Size)
            {
#if NETCOREAPP || NETSTANDARD2_1
                // Optimization on .NET Core App: Use the BufferPool to rent buffers
                if (Data != null)
                {
                    ArrayPool<byte>.Shared.Return(Data, clearArray: false);
                }

                Data = ArrayPool<byte>.Shared.Rent((int)Header.Size);
#else
                Data = new byte[Header.Size];
#endif
            }

            // followed by the actual framebuffer content
            _ = socket.Read(Data, (int)Header.Size);
        }

#if HAS_TASK
        /// <summary>
        /// Asynchronously refreshes the framebuffer: fetches the latest framebuffer data from the device. Access the <see cref="Header"/>
        /// and <see cref="Data"/> properties to get the updated framebuffer data.
        /// </summary>
        /// <param name="reset">Refreshes the header of framebuffer when <see langword="true"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public async Task RefreshAsync(bool reset = false, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();

            using IAdbSocket socket = Factories.AdbSocketFactory(EndPoint);
            // Select the target device
            socket.SetDevice(Device);

            // Send the framebuffer command
            socket.SendAdbRequest("framebuffer:");
            socket.ReadAdbResponse();

            // The result first is a FramebufferHeader object,
            _ = await socket.ReadAsync(headerData, cancellationToken).ConfigureAwait(false);

            if (reset || !headerInitialized)
            {
                Header = FramebufferHeader.Read(headerData);
                headerInitialized = true;
            }

            if (Data == null || Data.Length < Header.Size)
            {
#if NETCOREAPP || NETSTANDARD2_1
                // Optimization on .NET Core App: Use the BufferPool to rent buffers
                if (Data != null)
                {
                    ArrayPool<byte>.Shared.Return(Data, clearArray: false);
                }

                Data = ArrayPool<byte>.Shared.Rent((int)Header.Size);
#else
                Data = new byte[Header.Size];
#endif
            }

            // followed by the actual framebuffer content
            _ = await socket.ReadAsync(Data, (int)Header.Size, cancellationToken).ConfigureAwait(false);
        }
#endif

#if HAS_DRAWING
        /// <summary>
        /// Converts the framebuffer data to a <see cref="Bitmap"/>.
        /// </summary>
        /// <returns>An <see cref="Bitmap"/> which represents the framebuffer data.</returns>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public Bitmap ToImage()
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException("Call RefreshAsync first") : Header.ToImage(Data);
        }

        /// <inheritdoc/>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static explicit operator Image(Framebuffer value) => value.ToImage();

        /// <inheritdoc/>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static explicit operator Bitmap(Framebuffer value) => value.ToImage();
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
#if NETCOREAPP || NETSTANDARD2_1
                if (Data != null)
                {
                    ArrayPool<byte>.Shared.Return(Data, clearArray: false);
                }
#endif
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
        protected void EnsureNotDisposed() => ExceptionExtensions.ThrowIf(disposed, this);
    }
}
