using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    private static bool TryInvokeCoreAsync(uint threadId, Action action, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(threadId, new ActionInvokeClosure(action, null, cancellationToken));

    private static bool TryInvokeCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(threadId, new ActionInvokeClosure<TArg>(action, arg, null, cancellationToken));

    private static bool TryInvokeCoreAsync<TArg1, TArg2>(uint threadId,
        Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, null, cancellationToken));

    private static bool TryInvokeCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
        Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, null, cancellationToken));

    private static Task<bool>? TryInvokeTaskCoreAsync(uint threadId, Action action, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(threadId, new ActionInvokeClosure(action, completionSource, cancellationToken)) ? completionSource.Task : null;
    }

    private static Task<bool>? TryInvokeTaskCoreAsync<TArg>(uint threadId, Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(threadId, new ActionInvokeClosure<TArg>(action, arg, completionSource, cancellationToken)) ? completionSource.Task : null;
    }

    private static Task<bool>? TryInvokeTaskCoreAsync<TArg1, TArg2>(uint threadId,
        Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2>(action, arg1, arg2, completionSource, cancellationToken)) ? completionSource.Task : null;
    }

    private static Task<bool>? TryInvokeTaskCoreAsync<TArg1, TArg2, TArg3>(uint threadId,
        Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(threadId, new ActionInvokeClosure<TArg1, TArg2, TArg3>(action, arg1, arg2, arg3, completionSource, cancellationToken)) ? completionSource.Task : null;
    }
}
