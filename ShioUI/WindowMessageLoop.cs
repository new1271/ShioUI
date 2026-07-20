using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Structures;

using ShioUI.Internals.Native;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI;

public static partial class WindowMessageLoop
{
    private static readonly QueueStatusFlags StatusFlags = SystemHelper.IsWindows8OrHigher() ? QueueStatusFlags.AllInput : QueueStatusFlags.AllInputOld;

    private static readonly Action<NativeWindow> _windowShowAction = static window => window.Show();
    private static readonly UpdatableCollection<IWindowMessageFilter, UnwrappableList<IWindowMessageFilter>> _filters =
        UpdatableCollection.CreateUnwrapped<IWindowMessageFilter>();

    private static NativeWindow? _mainWindow;
    private static uint _invokeBarrier, _threadIdForMessageLoop;
    private static bool _isFirstTimeStart = true;

    public static event MessageLoopExceptionEventHandler? ExceptionCaught;

    public static bool HasMessageLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0;
        }
    }

    public static bool IsMessageLoopThread
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
            return messageLoopThreadId != 0 && NativeMethods.GetCurrentThreadId() == messageLoopThreadId;
        }
    }

    public static void ChangeMainWindow(NativeWindow mainWindow)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
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
                mainWindow.Show();
            else
                InvokeAsync(_windowShowAction, mainWindow);
        }
        NativeWindow? oldWindow = InterlockedHelper.Exchange(ref _mainWindow, mainWindow);
        if (oldWindow is not null)
            oldWindow.Destroyed -= OnWindowDestroyed;

        static void OnWindowDestroyed(object? sender, EventArgs e) => Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Start(NativeWindow mainWindow)
    {
        uint currentThreadId = NativeMethods.GetCurrentThreadId();
        if (InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, currentThreadId, 0) != 0)
            InvalidOperationException.Throw("Message loop is already exists!");
        if (_isFirstTimeStart)
        {
            _isFirstTimeStart = false;
            AddMessageFilter(InvokeMessageFilter.Instance);
        }
        else
        {
            InvokeMessageFilter.Instance.ProcessAllInvoke();
        }

        ChangeMainWindowCore(mainWindow, isMessageLoopThread: true);
        int result = DoMessageLoop();
        InterlockedHelper.CompareExchange(ref _threadIdForMessageLoop, 0, currentThreadId);

        ChangeMainWindowCore(null, isMessageLoopThread: false);
        return result;
    }

    internal static MessageLoopExceptionEventHandler? GetExceptionEventHandler() => ExceptionCaught;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe int DoMessageLoop()
    {
        PumpingMessage msg;
        SysBool32 status;
        while (status = User32.GetMessageW(&msg, IntPtr.Zero, 0u, 0u))
        {
            if (status.IsFailed)
                goto Failed;

            if (TryFilterMessage(ref msg, out nint result))
            {
                if (User32.InSendMessage())
                    User32.ReplyMessage(result);
            }
            else
            {
                User32.TranslateMessage(&msg);
                User32.DispatchMessageW(&msg);
            }
        }
        return unchecked((int)msg.wParam);

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
            IntPtr timerHandle = InterlockedHelper.Read(ref timerHandleBox.Value);
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
                            while (User32.PeekMessageW(&msg, IntPtr.Zero, 0u, 0u, PeekMessageOptions.Remove))
                            {
                                if (msg.message == WindowMessage.Quit)
                                    User32.PostQuitMessage(unchecked((int)msg.wParam));

                                if (TryFilterMessage(ref msg, out nint result))
                                {
                                    if (User32.InSendMessage())
                                        User32.ReplyMessage(result);
                                }
                                else
                                {
                                    User32.TranslateMessage(&msg);
                                    User32.DispatchMessageW(&msg);
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
            InterlockedHelper.Exchange(ref timerHandleBox.Value, IntPtr.Zero);
            Kernel32.CloseHandle(timerHandle);
        }
    }

    [Inline(InlineBehavior.Remove)]
    private static bool TryFilterMessage(ref PumpingMessage msg, out nint result)
    {
        UnwrappableList<IWindowMessageFilter> filters = _filters.Update();
        int count = filters.Count;
        if (count <= 0)
            goto Failed;

        IntPtr hwnd = msg.hwnd;
        WindowMessage message = msg.message;
        nint wParam = msg.wParam;
        nint lParam = msg.lParam;
        ref IWindowMessageFilter filterRef = ref UnsafeHelper.GetArrayDataReference(filters.Unwrap());
        for (nuint i = 0, limit = unchecked((nuint)count); i < limit; i++)
        {
            IWindowMessageFilter filter = UnsafeHelper.AddTypedOffset(ref filterRef, i);
            try
            {
                if (filter.TryProcessWindowMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam, out result))
                    return true;
            }
            catch (Exception ex)
            {
                MessageLoopExceptionEventHandler? eventHandler = ExceptionCaught;
                if (eventHandler is null)
                    throw;
                eventHandler.Invoke(filter, new MessageLoopExceptionEventArgs(ex));
            }
        }

    Failed:
        result = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Stop(int exitCode = 0) => User32.PostQuitMessage(exitCode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddMessageFilter(IWindowMessageFilter messageFilter) => _filters.Add(messageFilter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveMessageFilter(IWindowMessageFilter messageFilter) => _filters.Remove(messageFilter);
}
