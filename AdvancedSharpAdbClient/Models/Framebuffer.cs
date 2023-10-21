// <copyright file="Framebuffer.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides access to the framebuffer (that is, a copy of the image being displayed on the device screen).
    /// </summary>
    /// <param name="device">The device for which to fetch the frame buffer.</param>
    /// <param name="endPoint">The <see cref="EndPoint"/> at which the adb server is listening.</param>
    public class Framebuffer(DeviceData device, EndPoint endPoint) : IDisposable
    {
        private byte[] headerData = new byte[56];
        private bool headerInitialized;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Framebuffer"/> class.
        /// </summary>
        /// <param name="device">The device for which to fetch the frame buffer.</param>
        /// <param name="client">A <see cref="IAdbClient"/> which manages the connection with adb.</param>
        public Framebuffer(DeviceData device, IAdbClient client) : this(device, client?.EndPoint!) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Framebuffer"/> class.
        /// </summary>
        /// <param name="device">The device for which to fetch the frame buffer.</param>
        public Framebuffer(DeviceData device) : this(device, AdbClient.DefaultEndPoint) { }

        /// <summary>
        /// Gets the device for which to fetch the frame buffer.
        /// </summary>
        public DeviceData Device { get; } = device ?? throw new ArgumentNullException(nameof(device));

        /// <summary>
        /// Gets the <see cref="System.Net.EndPoint"/> at which the adb server is listening.
        /// </summary>
        public EndPoint EndPoint { get; init; } = endPoint ?? throw new ArgumentNullException(nameof(endPoint));

        /// <summary>
        /// Gets the framebuffer header. The header contains information such as the width and height and the color encoding.
        /// This property is set after you call <see cref="Refresh"/>.
        /// </summary>
        public FramebufferHeader Header { get; private set; }

        /// <summary>
        /// Gets the framebuffer data. You need to parse the <see cref="FramebufferHeader"/> to interpret this data (such as the color encoding).
        /// This property is set after you call <see cref="Refresh"/>.
        /// </summary>
        public byte[]? Data { get; private set; }

        /// <summary>
        /// Refreshes the framebuffer: fetches the latest framebuffer data from the device. Access the <see cref="Header"/>
        /// and <see cref="Data"/> properties to get the updated framebuffer data.
        /// </summary>
        /// <param name="reset">Refreshes the header of framebuffer when <see langword="true"/>.</param>
        [MemberNotNull(nameof(Data))]
        public virtual void Refresh(bool reset = false)
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
                Header = new FramebufferHeader(headerData);
                headerInitialized = true;
            }

            if (Data == null || Data.Length < Header.Size)
            {
#if HAS_BUFFERS
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
#if HAS_BUFFERS
            _ = socket.Read(Data.AsSpan(0, (int)Header.Size));
#else
            _ = socket.Read(Data, (int)Header.Size);
#endif
        }

#if HAS_TASK
        /// <summary>
        /// Asynchronously refreshes the framebuffer: fetches the latest framebuffer data from the device. Access the <see cref="Header"/>
        /// and <see cref="Data"/> properties to get the updated framebuffer data.
        /// </summary>
        /// <param name="reset">Refreshes the header of framebuffer when <see langword="true"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public virtual async Task RefreshAsync(bool reset = false, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();

            using IAdbSocket socket = Factories.AdbSocketFactory(EndPoint);
            // Select the target device
            await socket.SetDeviceAsync(Device, cancellationToken).ConfigureAwait(false);

            // Send the framebuffer command
            await socket.SendAdbRequestAsync("framebuffer:", cancellationToken).ConfigureAwait(false);
            await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            // The result first is a FramebufferHeader object,
            _ = await socket.ReadAsync(headerData, cancellationToken).ConfigureAwait(false);

            if (reset || !headerInitialized)
            {
                Header = new FramebufferHeader(headerData);
                headerInitialized = true;
            }

            if (Data == null || Data.Length < Header.Size)
            {
#if HAS_BUFFERS
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
#if HAS_BUFFERS
            _ = await socket.ReadAsync(Data.AsMemory(0, (int)Header.Size), cancellationToken).ConfigureAwait(false);
#else
            _ = await socket.ReadAsync(Data, (int)Header.Size, cancellationToken).ConfigureAwait(false);
#endif
        }
#endif

#if HAS_IMAGING
        /// <summary>
        /// Converts the framebuffer data to a <see cref="Bitmap"/>.
        /// </summary>
        /// <returns>An <see cref="Bitmap"/> which represents the framebuffer data.</returns>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public virtual Bitmap? ToImage()
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException($"Call {nameof(Refresh)} first") : Header.ToImage(Data);
        }

        /// <inheritdoc/>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static explicit operator Image?(Framebuffer value) => value.ToImage();

        /// <inheritdoc/>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static explicit operator Bitmap?(Framebuffer value) => value.ToImage();
#endif

#if WINDOWS_UWP
        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        public virtual Task<WriteableBitmap?> ToBitmap(CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException($"Call {nameof(RefreshAsync)} first") : Header.ToBitmap(Data, cancellationToken);
        }

        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="dispatcher">The target <see cref="CoreDispatcher"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        public virtual Task<WriteableBitmap?> ToBitmap(CoreDispatcher dispatcher, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException($"Call {nameof(RefreshAsync)} first") : Header.ToBitmap(Data, dispatcher, cancellationToken);
        }

        /// <summary>
        /// Converts the framebuffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="dispatcher">The target <see cref="DispatcherQueue"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>An <see cref="WriteableBitmap"/> which represents the framebuffer data.</returns>
        public virtual Task<WriteableBitmap?> ToBitmap(DispatcherQueue dispatcher, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            return Data == null ? throw new InvalidOperationException($"Call {nameof(RefreshAsync)} first") : Header.ToBitmap(Data, dispatcher, cancellationToken);
        }
#endif

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
#if HAS_BUFFERS
                if (Data != null)
                {
                    ArrayPool<byte>.Shared.Return(Data, clearArray: false);
                }
#endif
                headerData = null!;
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
        protected virtual void EnsureNotDisposed() => ExceptionExtensions.ThrowIf(disposed, this);
    }
}
