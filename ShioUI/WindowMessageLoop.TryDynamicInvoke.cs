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
    public static bool TryDynamicInvoke(Delegate @delegate, out object? result)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            goto Failed;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            result = @delegate.DynamicInvoke(null);
        }
        else
        {
            Task<object?>? task = TryDynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
            if (task is null)
                goto Failed;
            result = task.Result;
        }
        return true;

    Failed:
        result = default;
        return false;
    }

    public static bool TryDynamicInvoke(Delegate @delegate, object?[]? args, out object? result)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            goto Failed;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            result = @delegate.DynamicInvoke(args);
        }
        else
        {
            Task<object?>? task = TryDynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None);
            if (task is null)
                goto Failed;
            result = task.Result;
        }
        return true;

    Failed:
        result = default;
        return false;
    }

    public static bool TryDynamicInvokeAsync(Delegate @delegate)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        return TryDynamicInvokeCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryDynamicInvokeAsync(Delegate @delegate, params object?[]? args)
        => TryDynamicInvokeAsync(@delegate, args, CancellationToken.None);

    public static bool TryDynamicInvokeAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        return TryDynamicInvokeCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
    }

    public static Task<object?>? TryDynamicInvokeTaskAsync(Delegate @delegate)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(@delegate.DynamicInvoke(null))!;
        }
        else
            return TryDynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<object?>? TryDynamicInvokeTaskAsync(Delegate @delegate, params object?[]? args)
        => TryDynamicInvokeTaskAsync(@delegate, args, CancellationToken.None);

    public static Task<object?>? TryDynamicInvokeTaskAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return null;

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return Task.FromResult(@delegate.DynamicInvoke(args))!;
        }
        else
            return TryDynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
    }
}
