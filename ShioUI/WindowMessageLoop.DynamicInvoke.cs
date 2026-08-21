using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Native;

namespace ShioUI;

partial class WindowMessageLoop
{
    public static object? DynamicInvoke(Delegate @delegate)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke(); 
            return @delegate.DynamicInvoke(null);
        }
        return DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None).Result;
    }

    public static object? DynamicInvoke(Delegate @delegate, params object?[]? args)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return @delegate.DynamicInvoke(args);
        }
        return DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None).Result;
    }

    public static void DynamicInvokeAsync(Delegate @delegate)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        DynamicInvokeCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DynamicInvokeAsync(Delegate @delegate, params object?[]? args)
        => DynamicInvokeAsync(@delegate, args, CancellationToken.None);

    public static void DynamicInvokeAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        DynamicInvokeCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
    }

    public static Task<object?> DynamicInvokeTaskAsync(Delegate @delegate)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(@delegate.DynamicInvoke(null))!;
        }
        else
            return DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<object?> DynamicInvokeTaskAsync(Delegate @delegate, params object?[]? args)
        => DynamicInvokeTaskAsync(@delegate, args, CancellationToken.None);

    public static Task<object?> DynamicInvokeTaskAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(@delegate.DynamicInvoke(args))!;
        }
        else
            return DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
    }

    private static void DynamicInvokeCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default) 
        => PostInvokeClosure(threadId, new DynamicInvokeClosure(@delegate, args, null, cancellationToken));

    private static Task<object?> DynamicInvokeTaskCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new DynamicInvokeClosure(@delegate, args, completionSource, cancellationToken));
        return completionSource.Task;
    }
}
