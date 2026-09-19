using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? DynamicInvoke(Delegate @delegate)
        => TryDynamicInvoke(@delegate, out object? result) ? result : ThrowWhenMessageLoopThreadNotExists<object?>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? DynamicInvoke(Delegate @delegate, params object?[]? args)
        => TryDynamicInvoke(@delegate, args, out object? result) ? result : ThrowWhenMessageLoopThreadNotExists<object?>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DynamicInvokeAsync(Delegate @delegate)
        => ThrowWhenMessageLoopThreadNotExists(TryDynamicInvokeAsync(@delegate));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DynamicInvokeAsync(Delegate @delegate, params object?[]? args)
        => ThrowWhenMessageLoopThreadNotExists(TryDynamicInvokeAsync(@delegate, args));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DynamicInvokeAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryDynamicInvokeAsync(@delegate, args, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<object?> DynamicInvokeTaskAsync(Delegate @delegate)
        => ThrowWhenMessageLoopThreadNotExists(TryDynamicInvokeTaskAsync(@delegate));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<object?> DynamicInvokeTaskAsync(Delegate @delegate, params object?[]? args)
        => ThrowWhenMessageLoopThreadNotExists(TryDynamicInvokeTaskAsync(@delegate, args));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<object?> DynamicInvokeTaskAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryDynamicInvokeTaskAsync(@delegate, args, cancellationToken));
}
