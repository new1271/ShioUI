using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Buffers;
using RiceTea.Core.Collections;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Structures;

using ShioUI.Caching;
using ShioUI.Internals;
using ShioUI.Internals.Native;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI;

public static unsafe partial class WindowMessageLoop
{
    private static readonly QueueStatusFlags StatusFlags = SystemHelper.IsWindows8OrHigher() ? QueueStatusFlags.AllInput : QueueStatusFlags.AllInputOld;
    private static readonly Action<NativeWindow> _windowShowAction = static window => window.ShowCore();
    private static readonly Action<int> _stopAction = static exitCode =>
    {
        CoreWindow.DisposeAllWindows();
        User32.PostQuitMessage(exitCode);
    };
    private static readonly ArrayPool<IWindowMessageFilter> _windowMessageFilterPool = ArrayPool<IWindowMessageFilter>.Shared;
    private static readonly CacheStore<IWindowMessageFilter> _windowMessageFilterStore = new(null, &CreateSnapshotForWindowMessageFilter, &DropSnapshot);
    private static readonly SyncList<IWindowMessageFilter, UnwrappableList<IWindowMessageFilter>> _windowMessageFilters = new(new());

    private static NativeWindow? _mainWindow;
    private static ulong _windowMessageFiltersUpdateCounter;
    private static uint _invokeBarrier, _threadIdForMessageLoop;
    private static bool _isFirstTimeStart = true;

    public static event MessageLoopExceptionEventHandler? ExceptionCaught;

    public static bool HasMessageLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0;
        }
    }

    public static bool IsMessageLoopThread
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0 && NativeMethods.GetCurrentThreadId() == messageLoopThreadId;
        }
    }

    public static void ChangeMainWindow(NativeWindow? mainWindow)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw("The message loop is not exists!");
        ChangeMainWindowCore(mainWindow, IsMessageLoopThread);
    }

    private static void ChangeMainWindowCore(NativeWindow? mainWindow, bool isMessageLoopThread)
    {
        if (mainWindow is not null)
        {
            mainWindow.Destroyed += OnWindowDestroyed;
            if (isMessageLoopThread)
                mainWindow.ShowCore();
            else
                InvokeAsync(_windowShowAction, mainWindow);
        }
        NativeWindow? oldWindow = Atomics.Exchange(ref _mainWindow, mainWindow);
        if (oldWindow is not null && !ReferenceEquals(oldWindow, mainWindow))
            oldWindow.Destroyed -= OnWindowDestroyed;

        static void OnWindowDestroyed(object? sender, EventArgs e) => Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start() => Start(mainWindow: null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start(NativeWindow? mainWindow)
    {
        uint currentThreadId = NativeMethods.GetCurrentThreadId();
        if (Atomics.CompareExchange(ref _threadIdForMessageLoop, currentThreadId, 0) != 0)
            InvalidOperationException.Throw("Message loop is already exists!");
        if (_isFirstTimeStart)
        {
            _isFirstTimeStart = false;
            AddMessageFilter(InvokeMessageFilter.Instance);
        }
        else
        {
            ProcessAllInvoke();
        }

        ChangeMainWindowCore(mainWindow, isMessageLoopThread: true);
        int result;
        try
        {
            result = DoMessageLoop();
        }
        finally
        {
            Atomics.CompareExchange(ref _threadIdForMessageLoop, 0, currentThreadId);
            ChangeMainWindowCore(null, isMessageLoopThread: false);
        }
        return result;
    }

    public static MessageLoopExceptionEventHandler? GetExceptionEventHandler() => ExceptionCaught;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int DoMessageLoop()
    {
        PumpingMessage msg;
        PumpingMessage* pMsg = &msg;
        SysBool32 status;
        while (status = User32.GetMessageW(pMsg, IntPtr.Zero, 0u, 0u))
        {
            if (status.IsFailed)
                goto Failed;

            if (WindowMessageFilterHelper.TryFilterMessage(pMsg, _windowMessageFilterStore, out nint result))
            {
                if (User32.InSendMessage())
                    User32.ReplyMessage(result);
            }
            else
            {
                User32.TranslateMessage(pMsg);
                User32.DispatchMessageW(pMsg);
            }
        }
        return unchecked((int)msg.body.wParam);

    Failed:
        MessageLoopExceptionEventHandler? eventHandler = ExceptionCaught;
        if (eventHandler is null)
            Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
        else
        {
            Exception? exception = Marshal.GetExceptionForHR(Kernel32.GetLastError());
            if (exception is not null)
                eventHandler.Invoke(null, new MessageLoopExceptionEventArgs(exception));
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe void StartMiniLoop(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        InvokeMessageFilter.Instance.ProcessAllInvoke();

        IntPtr timerHandle = Kernel32.CreateWaitableTimerW(null, true, null);
        StrongBox<IntPtr> timerHandleBox = new StrongBox<IntPtr>(timerHandle);

        using CancellationTokenRegistration registration = cancellationToken.Register(static (state) =>
        {
            if (state is not StrongBox<IntPtr> timerHandleBox)
                return;
            IntPtr timerHandle = Atomics.Read(ref timerHandleBox.Value);
            if (timerHandle == IntPtr.Zero)
                return;

            long time = -1;
            Kernel32.SetWaitableTimer(timerHandle, &time, 0, null, null, false);
        }, timerHandleBox, useSynchronizationContext: true);

        try
        {
            while (true)
            {
                uint handleIndex = User32.MsgWaitForMultipleObjects(1, &timerHandle, false, uint.MaxValue, StatusFlags);
                switch (handleIndex)
                {
                    case 0:
                        return;
                    case 1:
                        {
                            PumpingMessage msg;
                            PumpingMessage* pMsg = &msg;
                            while (User32.PeekMessageW(pMsg, IntPtr.Zero, 0u, 0u, PeekMessageOptions.Remove))
                            {
                                if (msg.body.message == WindowMessage.Quit)
                                    User32.PostQuitMessage(unchecked((int)msg.body.wParam));

                                if (WindowMessageFilterHelper.TryFilterMessage(pMsg, _windowMessageFilterStore, out nint result))
                                {
                                    if (User32.InSendMessage())
                                        User32.ReplyMessage(result);
                                }
                                else
                                {
                                    User32.TranslateMessage(pMsg);
                                    User32.DispatchMessageW(pMsg);
                                }
                            }
                        }
                        break;
                    case uint.MaxValue:
                        Marshal.ThrowExceptionForHR(Kernel32.GetLastError());
                        return;
                    default:
                        InvalidOperationException.Throw("Invalid state!");
                        return;
                }
            }
        }
        finally
        {
            Atomics.Exchange(ref timerHandleBox.Value, IntPtr.Zero);
            Kernel32.CloseHandle(timerHandle);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Stop(int exitCode = 0) => InvokeAsync(_stopAction, exitCode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddMessageFilter(IWindowMessageFilter filter)
    {
        SyncList<IWindowMessageFilter, UnwrappableList<IWindowMessageFilter>> filters = _windowMessageFilters;

        using Lock.Scope scope = filters.EnterLockScope();
        filters.Remove(filter);
        filters.Add(filter);

        _windowMessageFilterStore.UpdateTimestamp(Atomics.Increment(ref _windowMessageFiltersUpdateCounter));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveMessageFilter(IWindowMessageFilter filter)
    {
        _windowMessageFilters.Remove(filter);
        _windowMessageFilterStore.UpdateTimestamp(Atomics.Increment(ref _windowMessageFiltersUpdateCounter));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CacheStore<IWindowMessageFilter>.Body CreateSnapshotForWindowMessageFilter(object? owner)
    {
        ArrayPool<IWindowMessageFilter> pool = _windowMessageFilterPool;
        (IWindowMessageFilter[] elements, int count) = pool.EnterRentScopeAndCapture(_windowMessageFilters);
        return new(elements, count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DropSnapshot(object? owner, in CacheStore<IWindowMessageFilter>.Body body)
    {
        ArrayPool<IWindowMessageFilter> pool = _windowMessageFilterPool;
        pool.Return(body.Array);
    }

}
