using System;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Internals;
using ShioUI.Internals.Native;

using GdiColor = System.Drawing.Color;
using GdiGraphics = System.Drawing.Graphics;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace ShioUI.Windows;

unsafe partial class NativeWindow
{
    bool IWindowMessageFilter.TryProcessWindowMessage(IntPtr handle, WindowMessage message, nint wParam, nint lParam, out nint result)
        => TryProcessWindowMessage(handle, message, wParam, lParam, out result);

    protected virtual bool TryProcessWindowMessage(IntPtr handle, WindowMessage message, nint wParam, nint lParam, out nint result)
    {
        if (message < WindowMessage.CustomClassMessageStart)
            return TryProcessSystemWindowMessage(handle, message, wParam, lParam, out result);

        if (message >= WindowMessage.RegisterWindowMessageStart && message <= WindowMessage.RegisterWindowMessageEnd)
            return TryProcessCustomWindowMessage(handle, (uint)message, wParam, lParam, out result);

        return TryProcessOtherWindowMessage(handle, (uint)message, wParam, lParam, out result);
    }

    protected virtual bool TryProcessSystemWindowMessage(IntPtr handle, WindowMessage message, nint wParam, nint lParam, out nint result)
    {
        result = 0;
        return message switch
        {
            WindowMessage.Activate => HandleActivate(wParam: wParam),
            WindowMessage.Close => HandleClose(),
            WindowMessage.Destroy => HandleDestroyed(),
            WindowMessage.NCLeftButtonDown => HandleNCLeftButtonDown(wParam: wParam),
            WindowMessage.NCLeftButtonUp => HandleNCLeftButtonUp(wParam: wParam),
            WindowMessage.SetText => HandleSetText(),
            WindowMessage.SetIcon => HandleSetIcon(),
            WindowMessage.SetCursor => HandleSetCursor(lParam: lParam),
            WindowMessage.WindowPositionChanging => HandleWindowPositionChanging(),
            WindowMessage.Sizing => HandleSizing(),
            WindowMessage.Size => HandleSize(wParam),
            WindowMessage.Paint => HandlePaint(),
            WindowMessage.EraseBackground => HandleEraseBackground(out result),
            WindowMessage.ShowWindow => HandleShowWindow(wParam: wParam, lParam: lParam),
            WindowMessage.SystemKeyDown => HandleSystemKeyDown(handle, wParam: wParam, lParam: lParam, out result),
            WindowMessage.SystemKeyUp => HandleSystemKeyUp(handle, wParam: wParam, lParam: lParam, out result),
            _ => false,
        };
    }

    [SkipLocalsInit]
    protected virtual bool TryProcessCustomWindowMessage(IntPtr handle, uint message, nint wParam, nint lParam, out nint result)
    {
        result = 0;
        return false;
    }

    [SkipLocalsInit]
    protected virtual bool TryProcessOtherWindowMessage(IntPtr handle, uint message, nint wParam, nint lParam, out nint result)
    {
        result = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool HandleActivate(nint wParam)
    {
        switch (wParam)
        {
            case 0: // WA_INACTIVE
                if ((Atomics.And(ref _windowFlags, ~(nuint)0b100) & 0b100) == 0b100)
                    OnFocusedChanged();
                break;
            case 1: // WA_ACTIVE
            case 2: // WA_CLICKACTIVE
                if ((Atomics.Or(ref _windowFlags, 0b100) & 0b100) != 0b100)
                    OnFocusedChanged();
                break;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleClose()
    {
        ClosingEventArgs args = new ClosingEventArgs((CloseReason)Atomics.Exchange(ref _closeReason, (uint)CloseReason.Unknown), cancelled: false);
        OnClosing(ref args);
        if (args.Cancelled)
            return true;
        OnClosed();
        IntPtr dialogParent = Atomics.Exchange(ref _dialogParent, IntPtr.Zero);
        if (dialogParent != IntPtr.Zero)
        {
            User32.EnableWindow(dialogParent, true);

            if (User32.IsWindowVisible(dialogParent))
                User32.SetActiveWindow(dialogParent);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleDestroyed()
    {
        if (Atomics.Exchange(ref _disposed, Booleans.TrueNativeUnsigned) == default)
            DisposeCore(disposing: true);
        
        if (Atomics.Exchange(ref _windowFlags, UnsafeHelper.GetMaxValue<nuint>()) != UnsafeHelper.GetMaxValue<nuint>())
        {
            IntPtr handle = _handleLazy.Value;
            if (handle == IntPtr.Zero)
                return true;
            if (!WindowClassImpl.Instance.TryUnregisterWindowUnsafe(handle, this))
                DebugHelper.Throw();
            CancellationTokenSource? dialogTokenSource = Atomics.Exchange(ref _dialogTokenSource, null);
            if (dialogTokenSource is not null)
            {
                try
                {
                    dialogTokenSource.Cancel(throwOnFirstException: false);
                }
                catch (Exception)
                {
                }
                finally
                {
                    dialogTokenSource.Dispose();
                }
            }
            OnDestroyed();
        }
        return true;
    }

    private static bool HandleNCLeftButtonDown(nint wParam)
        => (HitTestValue)wParam switch
        {
            HitTestValue.MinimizeButton or HitTestValue.MaximizeButton or HitTestValue.CloseButton => true,
            _ => false,
        };

    private bool HandleNCLeftButtonUp(nint wParam)
    {
        HitTestValue state = (HitTestValue)wParam;
        switch (state)
        {
            case HitTestValue.MinimizeButton:
                WindowState = WindowState.Minimized;
                return true;
            case HitTestValue.MaximizeButton:
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
                return true;
            case HitTestValue.CloseButton:
                Close(CloseReason.UserClicked);
                return true;
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleSetText()
    {
        Atomics.Exchange(ref _cachedTitle, null);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleSetIcon()
    {
        Atomics.Exchange(ref _cachedIcon, null);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleSetCursor(nint lParam)
    {
        switch ((HitTestValue)(ushort)lParam)
        {
            case HitTestValue.Client or HitTestValue.NoWhere:
                IntPtr oldHandle = User32.SetCursor(_cursor.Handle);
                if (oldHandle != IntPtr.Zero)
                    User32.DestroyCursor(oldHandle);
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleWindowPositionChanging()
    {
        _cachedBounds = default;
        Thread.MemoryBarrier();
        return false;
    }

    private bool HandleSizing()
    {
        OnResizing();
        return false;
    }

    private bool HandleSize(nint wParam)
    {
        switch (wParam)
        {
            case 2: // SIZE_MAXIMIZED
                {
                    WindowState oldState = (WindowState)Atomics.Exchange(ref _windowState, (uint)WindowState.Maximized);
                    if (oldState != WindowState.Maximized)
                        OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Maximized));
                }
                break;
            case 1: // SIZE_MINIMIZED
                {
                    WindowState oldState = (WindowState)Atomics.Exchange(ref _windowState, (uint)WindowState.Minimized);
                    if (oldState != WindowState.Minimized)
                        OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Minimized));
                }
                break;
            case 0: // SIZE_RESTORED
                {
                    WindowState oldState = (WindowState)Atomics.Exchange(ref _windowState, (uint)WindowState.Normal);
                    if (oldState != WindowState.Normal)
                        OnWindowStateChanged(new WindowStateChangedEventArgs(oldState, WindowState.Normal));
                }
                break;
            default:
                break;
        }
        OnResized();
        return false;
    }

    private bool HandlePaint()
    {
        IntPtr handle = Handle;
        if (handle == IntPtr.Zero)
            return true;
        PaintStruct paintStruct;
        IntPtr hdc = User32.BeginPaint(handle, &paintStruct);
        if (hdc == IntPtr.Zero)
            return true;
        using GdiGraphics graphics = GdiGraphics.FromHdc(hdc);
        graphics.Clear(GdiColor.Black);
        User32.EndPaint(handle, &paintStruct);
        return true;
    }

    private bool HandleShowWindow(nint wParam, nint lParam)
    {
        if (wParam != 0 && lParam == 0 && (Atomics.Or(ref _windowFlags, 0b10) & 0b10) != 0b10)
            WindowMessageLoop.InvokeAsync(OnShown);
        return false;
    }

    private bool HandleSystemKeyDown(IntPtr hwnd, nint wParam, nint lParam, out nint result)
    {
        if (wParam != (nint)VirtualKey.F10)
        {
            result = 0;
            return false;
        }
        return TryProcessSystemWindowMessage(hwnd, WindowMessage.KeyDown, wParam, lParam, out result);
    }

    private bool HandleSystemKeyUp(IntPtr hwnd, nint wParam, nint lParam, out nint result)
    {
        if (wParam != (nint)VirtualKey.F10)
        {
            result = 0;
            return false;
        }
        return TryProcessSystemWindowMessage(hwnd, WindowMessage.KeyUp, wParam, lParam, out result);
    }

    private static bool HandleEraseBackground(out nint result)
    {
        result = 1;
        return true;
    }
}
