using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    private static bool TryDynamicInvokeCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
        => TryPostInvokeClosure(threadId, new DynamicInvokeClosure(@delegate, args, null, cancellationToken));

    private static Task<object?>? TryDynamicInvokeTaskCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        return TryPostInvokeClosure(threadId, new DynamicInvokeClosure(@delegate, args, completionSource, cancellationToken)) ? completionSource.Task : null;
    }
}
