using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using InlineMethod;

using RiceTea.Core.Helpers;

using ShioUI.Internals;
using ShioUI.Internals.Native;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract partial class NativeWindow : CriticalFinalizerObject, IHwndOwner
{
    private static readonly Action<NativeWindow> ShowInternalAction = static (window) => window.ShowInternal();
    private static readonly Action<NativeWindow> ShowDialogCoreAction = static (window) => window.ShowDialogCore();

    private readonly GCHandle _parentReference;
    private readonly Lazy<IntPtr> _handleLazy;

    private CancellationTokenSource? _dialogTokenSource;
    private Win32ImageHandle _cursor;
    private Icon? _cachedIcon;
    private string? _cachedText;
    private Rectangle _cachedBounds;
    private IntPtr _dialogParent;
    /* Window flags
     * bit[0] = show() called or not
     * bit[1] = already shown or not
     * bit[2] = focused or not
     * (all bit set) = handle is destroyed
     */
    private nuint _windowFlags;
    private uint _windowState, _closeReason, _dialogResult;
    private bool _disposed;

    public NativeWindow(IHwndOwner? parent = null)
    {
        _parentReference = parent is null ? default : GCHandle.Alloc(parent, GCHandleType.Weak);
        _handleLazy = new Lazy<IntPtr>(() =>
        {
            GCHandle reference = _parentReference;
            IntPtr parentHandle;
            if (reference != default && reference.Target is IHwndOwner parent)
                parentHandle = parent.Handle;
            else
                parentHandle = IntPtr.Zero;
            return CreateWindowHandle(parentHandle);
        }, LazyThreadSafetyMode.None);
        _cursor = SystemCursors.Default;
        _dialogTokenSource = null;
    }

    public void WakeUp() => WindowMessageLoop.Invoke(WakeUpCore);

    public void Show()
    {
        if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
            return;
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
                ShowInternal();
            else
                WindowMessageLoop.Invoke(ShowInternalAction, this);
        }
        else
            WindowMessageLoop.Start(this);
    }

    public Task ShowAsync()
    {
        if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
            goto Completed;
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowInternal();
                goto Completed;
            }
            else
                return WindowMessageLoop.InvokeTaskAsync(ShowInternalAction, this);
        }
        else
        {
            WindowMessageLoop.Start(this);
            goto Completed;
        }

    Completed:
        return Task.CompletedTask;
    }

    public DialogResult ShowDialog()
    {
        if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
            return DialogResult.Invalid;
        if (WindowMessageLoop.HasMessageLoop)
        {
            if (WindowMessageLoop.IsMessageLoopThread)
            {
                WindowMessageLoop.ProcessAllInvoke();
                ShowDialogCore();
            }
            else
                WindowMessageLoop.InvokeTaskAsync(ShowDialogCoreAction, this).Wait();
        }
        else
        {
            WindowMessageLoop.Start(this);
        }
        return (DialogResult)InterlockedHelper.Read(ref _dialogResult);
    }

    public Task<DialogResult> ShowDialogAsync()
    {
        if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
            return Task.FromResult(DialogResult.Invalid);
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
        {
            WindowMessageLoop.Start(this);
        }
        return Task.FromResult((DialogResult)InterlockedHelper.Read(ref _dialogResult));

        async Task<DialogResult> AsyncCore()
        {
            await WindowMessageLoop.InvokeTaskAsync(ShowDialogCoreAction, this).ConfigureAwait(continueOnCapturedContext: false);
            return (DialogResult)InterlockedHelper.Read(ref _dialogResult);
        }
    }

    private void WakeUpCore()
    {
        IntPtr handle;
        if ((InterlockedHelper.Or(ref _windowFlags, 0b01) & 0b01) == 0b01)
            handle = Handle;
        else
            handle = ShowCore();
        if (User32.IsIconic(handle))
            User32.ShowWindow(handle, ShowWindowCommands.Restore);
        User32.SwitchToThisWindow(handle, fUnknown: true);
        User32.SetForegroundWindow(handle);
    }

    internal void ShowInternal() => ShowCore();

    private IntPtr ShowCore()
    {
        Lazy<IntPtr> handleLazy = _handleLazy;
        IntPtr handle;
        if (!handleLazy.IsValueCreated)
        {
            handle = handleLazy.Value;
            if (!WindowClassImpl.Instance.TryRegisterWindowUnsafe(handle, this))
                DebugHelper.Throw();
            OnHandleCreated(handle);
        }
        else
            handle = handleLazy.Value;

        ShowWindow(handle, WindowState.Normal);
        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ShowDialogInternal(NativeWindow window)
        => window.ShowDialogCore();

    private void ShowDialogCore()
    {
        IntPtr parent = FindParentHandleForDialog(handle: ShowCore());
        User32.EnableWindow(parent, false);
        InterlockedHelper.Write(ref _dialogParent, parent);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        InterlockedHelper.Write(ref _dialogTokenSource, tokenSource);
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
        InterlockedHelper.Write(ref _closeReason, (uint)reason);
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
}
