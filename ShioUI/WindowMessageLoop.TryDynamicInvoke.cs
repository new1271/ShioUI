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
        {
            result = default;
            return false;
        }

        if (NativeMethods.GetCurrentThreadId() == messageLoopThreadId)
        {
            ProcessAllInvoke();
            result = @delegate.DynamicInvoke(null);
        }
        else
            result = DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None).Result;
        return true;
    }

    public static bool TryDynamicInvoke(Delegate @delegate, object?[]? args, out object? result)
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
            result = @delegate.DynamicInvoke(args);
        }
        else
            result = DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None).Result;
        return true;
    }

    public static bool TryDynamicInvokeAsync(Delegate @delegate)
    {
        uint messageLoopThreadId = Atomics.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            return false;

        DynamicInvokeCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
        return true;
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

        DynamicInvokeCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
        return true;
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
            return DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
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
            return DynamicInvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
    }
}
