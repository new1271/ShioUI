using System;
using System.Threading;
using System.Threading.Tasks;

using RiceTea.Core;
using RiceTea.Core.Native;

namespace ShioUI;

partial class WindowMessageLoop
{
    public static bool TryInvoke(Action action)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke();
        }
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, CancellationToken.None).Wait();
        return true;
    }

    public static bool TryInvoke<TArg>(Action<TArg> action, TArg arg)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke(arg);
        }
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, arg, CancellationToken.None).Wait();
        return true;
    }

    public static bool TryInvoke<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke(arg1, arg2);
        }
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, CancellationToken.None).Wait();
        return true;
    }

    public static bool TryInvoke<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke(arg1, arg2, arg3);
        }
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, arg3, CancellationToken.None).Wait();
        return true;
    }

    public static bool TryInvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, action, cancellationToken);
        return true;
    }

    public static bool TryInvokeAsync<TArg>(Action<TArg> action,
        TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, action, arg, cancellationToken);
        return true;
    }

    public static bool TryInvokeAsync<TArg1, TArg2>(Action<TArg1, TArg2> action,
        TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, action, arg1, arg2, cancellationToken);
        return true;
    }

    public static bool TryInvokeAsync<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action,
        TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, action, arg1, arg2, arg3, cancellationToken);
        return true;
    }

    public static Task? TryInvokeTaskAsync(Action action, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke();
            return Task.CompletedTask;
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, action, cancellationToken);
    }

    public static Task? TryInvokeTaskAsync<TArg>(Action<TArg> action,
        TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke(arg);
            return Task.CompletedTask;
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, action, arg, cancellationToken);
    }

    public static Task? TryInvokeTaskAsync<TArg1, TArg2>(Action<TArg1, TArg2> action,
        TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke(arg1, arg2);
            return Task.CompletedTask;
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, cancellationToken);
    }

    public static Task? TryInvokeTaskAsync<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action,
        TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            action.Invoke(arg1, arg2, arg3);
            return Task.CompletedTask;
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, arg3, cancellationToken);
    }
}
