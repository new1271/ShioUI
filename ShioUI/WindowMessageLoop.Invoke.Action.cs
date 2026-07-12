using System;
using System.Threading;
using System.Threading.Tasks;

using RiceTea.Core.Helpers;

namespace ShioUI;

partial class WindowMessageLoop
{
    public static void Invoke(Action action)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (_threadIdLocal.Value == messageLoopThreadId)
            action.Invoke();
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, CancellationToken.None).Wait();
    }

    public static void Invoke<TArg>(Action<TArg> action, TArg arg)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (_threadIdLocal.Value == messageLoopThreadId)
            action.Invoke(arg);
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, arg, CancellationToken.None).Wait();
    }

    public static void Invoke<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (_threadIdLocal.Value == messageLoopThreadId)
            action.Invoke(arg1, arg2);
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, CancellationToken.None).Wait();
    }

    public static void Invoke<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (_threadIdLocal.Value == messageLoopThreadId)
            action.Invoke(arg1, arg2, arg3);
        else
            InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, arg3, CancellationToken.None).Wait();
    }

    public static void InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, action, cancellationToken);
    }

    public static void InvokeAsync<TArg>(Action<TArg> action, 
        TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, action, arg, cancellationToken);
    }

    public static void InvokeAsync<TArg1, TArg2>(Action<TArg1, TArg2> action, 
        TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, action, arg1, arg2, cancellationToken);
    }

    public static void InvokeAsync<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action,
        TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, action, arg1, arg2, arg3, cancellationToken);
    }

    public static Task InvokeTaskAsync(Action action, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        return InvokeTaskCoreAsync(messageLoopThreadId, action, cancellationToken);
    }

    public static Task InvokeTaskAsync<TArg>(Action<TArg> action, 
        TArg arg, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        return InvokeTaskCoreAsync(messageLoopThreadId, action, arg, cancellationToken);
    }

    public static Task InvokeTaskAsync<TArg1, TArg2>(Action<TArg1, TArg2> action, 
        TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        return InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, cancellationToken);
    }

    public static Task InvokeTaskAsync<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action,
        TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        return InvokeTaskCoreAsync(messageLoopThreadId, action, arg1, arg2, arg3, cancellationToken);
    }

    private static void InvokeCoreAsync(uint threadId, Action action, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        invokeMessageFilter.AddInvoke(new ActionInvokeClosure(action, null, cancellationToken));
        PostInvokeMessage(threadId);
    }

    private static void InvokeCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        invokeMessageFilter.AddInvoke(new ActionInvokeClosure<TArg>(action, arg, null, cancellationToken));
        PostInvokeMessage(threadId);
    }

    private static void InvokeCoreAsync<TArg1, TArg2>(uint threadId,
        Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        invokeMessageFilter.AddInvoke(new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, null, cancellationToken));
        PostInvokeMessage(threadId);
    }

    private static void InvokeCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
        Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        invokeMessageFilter.AddInvoke(new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, null, cancellationToken));
        PostInvokeMessage(threadId);
    }

    private static async Task InvokeTaskCoreAsync(uint threadId, Action action, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        invokeMessageFilter.AddInvoke(new ActionInvokeClosure(action, completionSource, cancellationToken));
        PostInvokeMessage(threadId);
        await completionSource.Task;
    }

    private static async Task InvokeTaskCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        invokeMessageFilter.AddInvoke(new ActionInvokeClosure<TArg>(action, arg, completionSource, cancellationToken));
        PostInvokeMessage(threadId);
        await completionSource.Task;
    }

    private static async Task InvokeTaskCoreAsync<TArg1, TArg2>(uint threadId,
        Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        invokeMessageFilter.AddInvoke(new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, completionSource, cancellationToken));
        PostInvokeMessage(threadId);
        await completionSource.Task;
    }

    private static async Task InvokeTaskCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
        Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        invokeMessageFilter.AddInvoke(new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, completionSource, cancellationToken));
        PostInvokeMessage(threadId);
        await completionSource.Task;
    }
}
