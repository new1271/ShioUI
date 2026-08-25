using System;
using System.Drawing;
using System.Runtime.InteropServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Internals;
using ShioUI.Internals.Native;

namespace ShioUI.Windows;

partial class NativeWindow : IDisposable
#if NET8_0_OR_GREATER
    , IAsyncDisposable
#endif
{
    private unsafe IntPtr CreateWindowHandle(IntPtr parent)
    {
        WindowClassImpl windowClass = WindowClassImpl.Instance;
        CreateWindowInfo windowInfo = GetCreateWindowInfo();

        IntPtr result = User32.CreateWindowExW(
            lpClassName: (char*)windowClass.Atom,
            lpWindowName: null,
            dwStyle: windowInfo.Styles,
            dwExStyle: windowInfo.ExtendedStyles,
            X: windowInfo.X, Y: windowInfo.Y,
            nWidth: windowInfo.Width, nHeight: windowInfo.Height,
            hWndParent: parent,
            hMenu: IntPtr.Zero,
            hInstance: windowClass.HInstance,
            lpParam: null);
        if (result == IntPtr.Zero)
            Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
        return result;
    }

    protected virtual CreateWindowInfo GetCreateWindowInfo()
    {
        const int CW_USEDEFAULT = unchecked((int)0x80000000);
        return new CreateWindowInfo(
            styles: WindowStyles.OverlappedWindow,
            extendedStyles: WindowExtendedStyles.AppWindow | WindowExtendedStyles.WindowEdge,
            x: CW_USEDEFAULT,
            y: CW_USEDEFAULT,
            width: CW_USEDEFAULT,
            height: CW_USEDEFAULT);
    }

    protected virtual void OnHandleCreated(IntPtr handle)
    {
        string text = Atomics.CompareExchange(ref _cachedTitle, nameof(NativeWindow), null) ?? nameof(NativeWindow);
        User32.SetWindowText(handle, text);
        Icon? icon = Atomics.Read(ref _cachedIcon);
        IntPtr iconHandle = icon is null ? IntPtr.Zero : User32.CopyIcon(icon.Handle);
        SetIconCore(handle, iconHandle);
    }

    private void DisposeInternal(bool disposing)
    {
        if (Atomics.Exchange(ref _disposed, UnsafeHelper.GetMaxValue<nuint>()) != 0)
            return;
        WindowMessageLoop.Invoke(static (_this, disposing) => _this.DisposeSync(disposing), this, disposing);
    }

    private void DisposeSync(bool disposing)
    {
        try
        {
            DisposeCore(disposing);
        }
        finally
        {
            IntPtr handle = _handle;
            if (handle != IntPtr.Zero)
            {
                Atomics.Write(ref _handle, IntPtr.Zero);
                User32.DestroyWindow(handle);
            }
            RuntimeFlags = WindowRuntimeFlags.Destroyed;
        }
    }

    protected virtual void DisposeCore(bool disposing) { }

    ~NativeWindow() => DisposeInternal(disposing: false);

    public void Dispose()
    {
        DisposeInternal(disposing: true);
        GC.SuppressFinalize(this);
    }

#if NET8_0_OR_GREATER
    private System.Threading.Tasks.Task DisposeInternalAsync()
        => WindowMessageLoop.InvokeTaskAsync(static (_this) => _this.DisposeSync(disposing: true), this);

    public async System.Threading.Tasks.ValueTask DisposeAsync()
    {
        await DisposeInternalAsync();
        GC.SuppressFinalize(this);
    }
#endif
}
