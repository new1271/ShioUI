using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Extensions;

using ShioUI.Internals;
using ShioUI.Internals.Native;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract partial class NativeWindow : CriticalFinalizerObject, IHwndOwner
{
    private static readonly Action<NativeWindow> WakeUpCoreAction = static (window) => window.WakeUpCore();
    private static readonly Action<NativeWindow> ShowCoreAction = static (window) => window.ShowCore();
    private static readonly Action<NativeWindow> HideCoreAction = static (window) => window.HideCore();
    private static readonly Action<NativeWindow> ShowDialogCoreAction = static (window) => window.ShowDialogCore();

    private readonly GCHandle _parentReference;

    private CancellationTokenSource? _dialogTokenSource;
    private Win32ImageHandle _cursor;
    private Icon? _cachedIcon;
    private string? _cachedTitle;
    private Rectangle _cachedBounds;
    private IntPtr _handle, _dialogParent;
    private nuint _disposed;
    private uint _runtimeFlags, _windowState, _closeReason, _dialogResult;

    public NativeWindow(IHwndOwner? parent = null)
    {
        _parentReference = parent is null ? default : GCHandle.Alloc(parent, GCHandleType.Weak);
        _cursor = SystemCursors.Default;
        _dialogTokenSource = null;
    }

    public void WakeUp()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                WakeUpCore();
            }
            else
                WindowMessageLoop.Invoke(WakeUpCoreAction, this);
        }
        else
            WindowMessageLoop.Start(this);
    }

    public Task WakeUpAsync()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                WakeUpCore();
            }
            else
                return WindowMessageLoop.InvokeTaskAsync(WakeUpCoreAction, this);
        }
        else
            WindowMessageLoop.Start(this);

        return Task.CompletedTask;
    }

    public void Hide()
    {
        if (!WindowMessageLoop.HasMessageLoop)
            return;

        if (WindowMessageLoop.IsMessageLoopThread)
        {
            WindowMessageLoop.ProcessAllInvoke();
            HideCore();
        }
        else
            WindowMessageLoop.Invoke(HideCoreAction, this);
    }

    public Task HideAsync()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                HideCore();
            }
            else
                return WindowMessageLoop.InvokeTaskAsync(HideCoreAction, this);
        }

        return Task.CompletedTask;
    }

    public void Show()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowCore();
            }
            else
                WindowMessageLoop.Invoke(ShowCoreAction, this);
        }
        else
            WindowMessageLoop.Start(this);
    }

    public Task ShowAsync()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowCore();
            }
            else
                return WindowMessageLoop.InvokeTaskAsync(ShowCoreAction, this);
        }
        else
            WindowMessageLoop.Start(this);

        return Task.CompletedTask;
    }

    public DialogResult ShowDialog()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowDialogCore();
            }
            else
                WindowMessageLoop.Invoke(ShowDialogCoreAction, this);
        }
        else
        {
            WindowMessageLoop.Start(this);
        }
        return (DialogResult)Atomics.Read(ref _dialogResult);
    }

    public Task<DialogResult> ShowDialogAsync()
    {
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowDialogCore();
            }
            else
                return AsyncCore();
        }
        else
            WindowMessageLoop.Start(this);
        return Task.FromResult((DialogResult)Atomics.Read(ref _dialogResult));

        async Task<DialogResult> AsyncCore()
        {
            await WindowMessageLoop.InvokeTaskAsync(ShowDialogCoreAction, this);
            return (DialogResult)Atomics.Read(ref _dialogResult);
        }
    }

    private void WakeUpCore()
    {
        IntPtr handle = GetOrCreateWindowHandle();
        if (User32.IsIconic(handle))
            User32.ShowWindow(handle, ShowWindowCommands.Restore);
        User32.SwitchToThisWindow(handle, fUnknown: true);
        User32.SetForegroundWindow(handle);
    }

    private IntPtr HideCore()
    {
        IntPtr handle = _handle;
        if (handle == IntPtr.Zero)
            return IntPtr.Zero;

        HideCore(handle);
        return handle;
    }

    protected virtual void HideCore(IntPtr handle) => User32.ShowWindow(handle, ShowWindowCommands.Hide);

    internal IntPtr ShowCore()
    {
        IntPtr handle = GetOrCreateWindowHandle();
        ShowCore(handle);
        return handle;
    }

    protected virtual void ShowCore(IntPtr handle) => User32.ShowWindow(handle, ShowWindowCommands.Normal);

    internal void ShowDialogCore()
    {
        IntPtr parent = FindParentHandleForDialog(handle: ShowCore());
        User32.EnableWindow(parent, false);
        Atomics.Write(ref _dialogParent, parent);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Atomics.Write(ref _dialogTokenSource, tokenSource);
        WindowMessageLoop.StartMiniLoop(tokenSource.Token);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close() => Close(CloseReason.Programmically);

    public void Close(CloseReason reason)
    {
        IntPtr handle = Handle;
        if (handle == IntPtr.Zero)
            return;
        Atomics.Write(ref _closeReason, (uint)reason);
        User32.PostMessageW(handle, WindowMessage.Close, 0, 0);
    }

    private static IntPtr FindParentHandleForDialog(IntPtr handle)
    {
        IntPtr parent = User32.GetWindow(handle, GetWindowCommand.Owner);
        if (parent != IntPtr.Zero)
            return parent;

        const int GWLP_HWNDPARENT = -8;

        parent = User32.GetActiveWindow();
        User32.SetWindowLongPtrW(handle, GWLP_HWNDPARENT, parent);
        return parent;
    }

    private IntPtr GetOrCreateWindowHandle()
    {
        IntPtr handle;
        if (GetRuntimeFlagsDirectly().HasFlagFast(WindowRuntimeFlags.Initialized))
        {
            handle = _handle;
        }
        else
        {
            GCHandle reference = _parentReference;
            IntPtr parentHandle;
            if (reference != default && reference.Target is IHwndOwner parent)
                parentHandle = parent.Handle;
            else
                parentHandle = IntPtr.Zero;

            handle = CreateWindowHandle(parentHandle);

            if (handle == IntPtr.Zero)
                InvalidOperationException.Throw("Cannot create the window!");

            if (!WindowClassImpl.Instance.TryRegisterWindowUnsafe(handle, this))
                InvalidOperationException.Throw("Cannot register the window!");
            Atomics.Write(ref _handle, handle);
            OnHandleCreated(handle);
        }

        return handle;
    }
}
