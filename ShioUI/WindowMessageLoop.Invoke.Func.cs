using System;
using System.Threading;
using System.Threading.Tasks;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI;

partial class WindowMessageLoop
{
    public static TResult Invoke<TResult>(Func<TResult> func)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<TResult>();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return func.Invoke();
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, CancellationToken.None).Result;
    }

    public static TResult Invoke<TArg, TResult>(Func<TArg, TResult> func, TArg arg)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<TResult>();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return func.Invoke(arg);
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, arg, CancellationToken.None).Result;
    }

    public static TResult Invoke<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<TResult>();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return func.Invoke(arg1, arg2);
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, CancellationToken.None).Result;
    }

    public static TResult Invoke<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<TResult>();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return func.Invoke(arg1, arg2, arg3);
        }
        else
            return InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, arg3, CancellationToken.None).Result;
    }

    public static void InvokeAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, func, cancellationToken);
    }

    public static void InvokeAsync<TArg, TResult>(Func<TArg, TResult> func,
        TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, func, arg, cancellationToken);
    }

    public static void InvokeAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func,
        TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, func, arg1, arg2, cancellationToken);
    }

    public static void InvokeAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func,
        TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, func, arg1, arg2, arg3, cancellationToken);
    }

    public static Task<TResult> InvokeTaskAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<Task<TResult>>();

        return InvokeTaskCoreAsync(messageLoopThreadId, func, cancellationToken);
    }

    public static Task<TResult> InvokeTaskAsync<TArg, TResult>(Func<TArg, TResult> func,
        TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<Task<TResult>>();

        return InvokeTaskCoreAsync(messageLoopThreadId, func, arg, cancellationToken);
    }

    public static Task<TResult> InvokeTaskAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func,
        TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<Task<TResult>>();

        return InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, cancellationToken);
    }

    public static Task<TResult> InvokeTaskAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func,
        TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return InvalidOperationException.Throw<Task<TResult>>();

        return InvokeTaskCoreAsync(messageLoopThreadId, func, arg1, arg2, arg3, cancellationToken);
    }

    private static void InvokeCoreAsync<TResult>(uint threadId, Func<TResult> func, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new FuncInvokeClosure<TResult>(func, null, cancellationToken));

    private static void InvokeCoreAsync<TArg, TResult>(uint threadId, Func<TArg, TResult> func, TArg arg, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new FuncInvokeClosure<TArg, TResult>(func, arg, null, cancellationToken));

    private static void InvokeCoreAsync<TArg1, TArg2, TResult>(uint threadId,
        Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new FuncInvokeClosure<TArg1, TArg2, TResult>(func, arg1, arg2, null, cancellationToken));

    private static void InvokeCoreAsync<TArg1, TArg2, TArg3, TResult>(uint threadId,
        Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new FuncInvokeClosure<TArg1, TArg2, TArg3, TResult>(func, arg1, arg2, arg3, null, cancellationToken));

    private static Task<TResult> InvokeTaskCoreAsync<TResult>(uint threadId, Func<TResult> func, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new FuncInvokeClosure<TResult>(func, completionSource, cancellationToken));
        return completionSource.Task;
    }

    private static Task<TResult> InvokeTaskCoreAsync<TArg, TResult>(uint threadId, Func<TArg, TResult> func, TArg arg, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new FuncInvokeClosure<TArg, TResult>(func, arg, completionSource, cancellationToken));
        return completionSource.Task;
    }

    private static Task<TResult> InvokeTaskCoreAsync<TArg1, TArg2, TResult>(uint threadId,
        Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new FuncInvokeClosure<TArg1, TArg2, TResult>(func, arg1, arg2, completionSource, cancellationToken));
        return completionSource.Task;
    }

    private static Task<TResult> InvokeTaskCoreAsync<TArg1, TArg2, TArg3, TResult>(uint threadId,
        Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new FuncInvokeClosure<TArg1, TArg2, TArg3, TResult>(func, arg1, arg2, arg3, completionSource, cancellationToken));
        return completionSource.Task;
    }
}
