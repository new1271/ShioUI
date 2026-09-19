using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    private static void DynamicInvokeCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default) 
        => PostInvokeClosure(threadId, new DynamicInvokeClosure(@delegate, args, null, cancellationToken));

    private static Task<object?> DynamicInvokeTaskCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new DynamicInvokeClosure(@delegate, args, completionSource, cancellationToken));
        return completionSource.Task;
    }
}
