using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using RiceTea.Core;
using RiceTea.Core.Native;

namespace ShioUI;

partial class WindowMessageLoop
{
    public static bool TryInvoke<TResult>(Func<TResult> func, [MaybeNullWhen(false)] out TResult result)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
        {
            result = default;
            return false;
        }

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            result = func.Invoke();
        }
        else
            result = InvokeTaskCoreAsync(messageLoopThreadId, func, CancellationToken.None).Result;
        return true;
    }

    public static bool TryInvoke<TArg, TResult>(Func<TArg, TResult> func, TArg arg, [MaybeNullWhen(false)] out TResult result)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
        {
            result = default;
            return false;
        }

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            result = func.Invoke(arg);
        }
        else
            result = InvokeTaskCoreAsync(messageLoopThreadId, func, arg, CancellationToken.None).Result;
        return true;
    }

    public static bool TryInvoke<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2, [MaybeNullWhen(false)] out TResult result)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
        {
            result = default;
            return false;
        }

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            result = func.Invoke(arg1, arg2);
        }
        else
            result = InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, CancellationToken.None).Result;
        return true;
    }

    public static bool TryInvoke<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3, [MaybeNullWhen(false)] out TResult result)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
        {
            result = default;
            return false;
        }

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            result = func.Invoke(arg1, arg2, arg3);
        }
        else
            result = InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, arg3, CancellationToken.None).Result;
        return true;
    }

    public static bool TryInvokeAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, func, cancellationToken);
        return true;
    }

    public static bool TryInvokeAsync<TArg, TResult>(Func<TArg, TResult> func, TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, func, arg, cancellationToken);
        return true;
    }

    public static bool TryInvokeAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, func, arg1, arg2, cancellationToken);
        return true;
    }

    public static bool TryInvokeAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        InvokeCoreAsync(messageLoopThreadId, func, arg1, arg2, arg3, cancellationToken);
        return true;
    }

    public static Task<TResult>? TryInvokeTaskAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(func.Invoke());
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, cancellationToken);
    }

    public static Task<TResult>? TryInvokeTaskAsync<TArg, TResult>(Func<TArg, TResult> func,
        TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(func.Invoke(arg));
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, arg, cancellationToken);
    }

    public static Task<TResult>? TryInvokeTaskAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func,
        TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(func.Invoke(arg1, arg2));
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, cancellationToken);
    }

    public static Task<TResult>? TryInvokeTaskAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func,
        TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(func.Invoke(arg1, arg2, arg3));
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, arg3, cancellationToken);
    }
}
