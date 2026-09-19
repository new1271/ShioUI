using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    private static void InvokeCoreAsync(uint threadId, Action action, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new ActionInvokeClosure(action, null, cancellationToken));

    private static void InvokeCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new ActionInvokeClosure<TArg>(action, arg, null, cancellationToken));

    private static void InvokeCoreAsync<TArg1, TArg2>(uint threadId,
        Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, null, cancellationToken));

    private static void InvokeCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
        Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => PostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, null, cancellationToken));

    private static Task<bool> InvokeTaskCoreAsync(uint threadId, Action action, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new ActionInvokeClosure(action, completionSource, cancellationToken));
        return completionSource.Task;
    }

    private static Task<bool> InvokeTaskCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new ActionInvokeClosure<TArg>(action, arg, completionSource, cancellationToken));
        return completionSource.Task;
    }

    private static Task<bool> InvokeTaskCoreAsync<TArg1, TArg2>(uint threadId,
        Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, completionSource, cancellationToken));
        return completionSource.Task;
    }

    private static Task<bool> InvokeTaskCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
        Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, completionSource, cancellationToken));
        return completionSource.Task;
    }
}
