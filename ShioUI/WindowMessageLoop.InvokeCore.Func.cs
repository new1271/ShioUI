using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
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
