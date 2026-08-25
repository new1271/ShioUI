using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Internals.Native;
using ShioUI.Windows;

namespace ShioUI.Internals;

internal sealed unsafe class WindowClassImpl
{
    public static readonly WindowClassImpl Instance;

#if NET472_OR_GREATER
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate nint WndProcDelegate(IntPtr hwnd, uint message, nint lParam, nint wParam);
    private static readonly WndProcDelegate? _wndProcDelegate;
#endif

    private readonly Dictionary<IntPtr, IHwndOwner> _hwndOwnerDict = new();
    private readonly IntPtr _hInstance;
    private readonly ushort _atom;

    private nuint _barrier;

    static WindowClassImpl()
    {
        void* wndProcFunc;

#if NET8_0_OR_GREATER
        wndProcFunc = (delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint>)&ProcessWindowMessage;
#else
        WndProcDelegate wndProcDelegate = ProcessWindowMessage;
        _wndProcDelegate = wndProcDelegate;
        wndProcFunc = (delegate* unmanaged[Stdcall]<IntPtr, uint, nint, nint, nint>)Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
#endif

        Instance = new WindowClassImpl(wndProcFunc);
    }

    private WindowClassImpl(void* wndProcFunc)
    {
        ushort atom;
        IntPtr hInstance = Kernel32.GetModuleHandleW(null);
        fixed (char* className = "ShioWindow")
        {
            WindowClassEx clazz = new WindowClassEx()
            {
                cbSize = UnsafeHelper.SizeOf<WindowClassEx>(),
                style = ClassStyles.OwnDC,
                hInstance = hInstance,
                lpfnWndProc = wndProcFunc,
                lpszClassName = className,
                hbrBackground = Gdi32.CreateSolidBrush(0x00000000)
            };

            atom = User32.RegisterClassExW(&clazz);
            if (atom == 0)
                throw new Win32Exception(Kernel32.GetLastError());
        }

        _hInstance = hInstance;
        _atom = atom;
    }

    public ushort Atom => _atom;
    public IntPtr HInstance => _hInstance;

#if NET8_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint ProcessWindowMessage(IntPtr hwnd, uint message, nint wParam, nint lParam)
    {
        WindowClassImpl instance = Instance;
        try
        {
            if (instance.TryProcessWindowMessage(hwnd, message, wParam, lParam, out nint result))
                return result;
        }
        catch (Exception ex)
        {
            MessageLoopExceptionEventHandler? eventHandler = WindowMessageLoop.GetExceptionEventHandler();
            if (eventHandler is null)
                throw;
            eventHandler.Invoke(null, new MessageLoopExceptionEventArgs(ex));
        }
        return User32.DefWindowProcW(hwnd, message, wParam, lParam);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnterBarrier()
    {
        ref nuint barrier = ref _barrier;
        while (Atomics.Exchange(ref barrier, 1) != 0)
        {
            SpinWait wait = new SpinWait();
            while (Atomics.Read(ref barrier) != 0)
                wait.SpinOnce();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExitBarrier() => Atomics.Exchange(ref _barrier, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRegisterWindow<T>(T owner) where T : IHwndOwner
        => TryRegisterWindowUnsafe(owner.Handle, owner);

    public bool TryRegisterWindowUnsafe<T>(IntPtr handle, T owner) where T : IHwndOwner
    {
        if (handle == IntPtr.Zero)
            return false;

        Dictionary<IntPtr, IHwndOwner> dict = _hwndOwnerDict;
        EnterBarrier();
        try
        {
            if (!dict.TryGetValue(handle, out IHwndOwner? target))
            {
                dict.Add(handle, owner);
                return true;
            }
            if (ReferenceEquals(target, owner))
                return true;
            if (target is null || target.Handle == owner.Handle)
            {
                dict[handle] = owner;
                return true;
            }
            return false;
        }
        finally
        {
            ExitBarrier();
        }
    }

    public bool TryUnregisterWindow<T>(T owner) where T : IHwndOwner
        => TryUnregisterWindowUnsafe(owner.Handle, owner);

    public bool TryUnregisterWindowUnsafe<T>(IntPtr handle, T owner) where T : IHwndOwner
    {
        if (handle == IntPtr.Zero)
            return false;

        Dictionary<IntPtr, IHwndOwner> dict = _hwndOwnerDict;
        EnterBarrier();
        try
        {
            if (!dict.TryGetValue(handle, out IHwndOwner? target))
                return false;
            if (ReferenceEquals(target, owner))
            {
                dict.Remove(handle);
                return true;
            }
            if (target is null || target.Handle == owner.Handle)
                dict.Remove(handle);
            return false;
        }
        finally
        {
            ExitBarrier();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryProcessWindowMessage(IntPtr hwnd, uint message, nint wParam, nint lParam, out nint result)
    {
        IHwndOwner? owner;
        EnterBarrier();
        try
        {
            if (!_hwndOwnerDict.TryGetValue(hwnd, out owner))
                goto Failed;
        }
        finally
        {
            ExitBarrier();
        }

        return owner.TryProcessWindowMessage(hwnd, (WindowMessage)message, wParam, lParam, out result);

    Failed:
        result = 0;
        return false;
    }
}
