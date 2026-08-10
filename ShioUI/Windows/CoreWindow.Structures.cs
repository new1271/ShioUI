using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ShioUI.Graphics;
using ShioUI.Utils;

namespace ShioUI.Windows;

partial class CoreWindow
{
    [StructLayout(LayoutKind.Auto)]
    protected ref struct WindowLayoutData
    {
        public Rectangle MinimizeButtonBounds, MaximizeButtonBounds, CloseButtonBounds, PageBounds, TitleBarBounds;
        public Point DrawingOffset;
        public int ActiveBorderWidth;

        public Point MinimizeButtonLocation
        {
            readonly get => MinimizeButtonBounds.Location;
            set => MinimizeButtonBounds.Location = value;
        }

        public Size MinimizeButtonSize
        {
            readonly get => MinimizeButtonBounds.Size;
            set => MinimizeButtonBounds.Size = value;
        }

        public Point MaximizeButtonLocation
        {
            readonly get => MaximizeButtonBounds.Location;
            set => MaximizeButtonBounds.Location = value;
        }

        public Size MaximizeButtonSize
        {
            readonly get => MaximizeButtonBounds.Size;
            set => MaximizeButtonBounds.Size = value;
        }

        public Point CloseButtonLocation
        {
            readonly get => CloseButtonBounds.Location;
            set => CloseButtonBounds.Location = value;
        }

        public Size CloseButtonSize
        {
            readonly get => CloseButtonBounds.Size;
            set => CloseButtonBounds.Size = value;
        }

        public Point TitleBarLocation
        {
            readonly get => TitleBarBounds.Location;
            set => TitleBarBounds.Location = value;
        }

        public Size TitleBarSize
        {
            readonly get => TitleBarBounds.Size;
            set => TitleBarBounds.Size = value;
        }

        public Point PageLocation
        {
            readonly get => PageBounds.Location;
            set => PageBounds.Location = value;
        }

        public Size PageSize
        {
            readonly get => PageBounds.Size;
            set => PageBounds.Size = value;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    protected ref struct WindowRenderingData
    {
        public WindowLayoutData Layout;
        public ulong ResizeFramestamp, LastRenderTimestamp, CurrentRenderTimestamp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RenderInformation CreateRenderInformation(bool ignoreNeedRefresh)
            => new RenderInformation(ignoreNeedRefresh, ResizeFramestamp, LastRenderTimestamp, CurrentRenderTimestamp);
    }

    [StructLayout(LayoutKind.Auto)]
    public ref struct BatchUpdateScope : IDisposable
    {
        private RenderingController? _controller;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BatchUpdateScope(RenderingController controller)
        {
            controller.Lock();
            _controller = controller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            _controller = null;
            controller.RequestUpdate(false);
            controller.Unlock();
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public ref struct CriticalUpdateScope : IDisposable
    {
        private RenderingController? _controller;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CriticalUpdateScope(RenderingController controller)
        {
            controller.Lock();
            controller.WaitForRendering();
            _controller = controller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            RenderingController? controller = _controller;
            if (controller is null)
                return;
            _controller = null;
            controller.RequestUpdate(false);
            controller.Unlock();
        }
    }
}
