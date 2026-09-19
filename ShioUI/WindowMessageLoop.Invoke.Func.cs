using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Invoke<TResult>(Func<TResult> func)
        => TryInvoke(func, out TResult? result) ? result : ThrowWhenMessageLoopThreadNotExists<TResult>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Invoke<TArg, TResult>(Func<TArg, TResult> func, TArg arg)
        => TryInvoke(func, arg, out TResult? result) ? result : ThrowWhenMessageLoopThreadNotExists<TResult>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Invoke<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2)
        => TryInvoke(func, arg1, arg2, out TResult? result) ? result : ThrowWhenMessageLoopThreadNotExists<TResult>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Invoke<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        => TryInvoke(func, arg1, arg2, arg3, out TResult? result) ? result : ThrowWhenMessageLoopThreadNotExists<TResult>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(func, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync<TArg, TResult>(Func<TArg, TResult> func, TArg arg, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(func, arg, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(func, arg1, arg2, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(func, arg1, arg2, arg3, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> InvokeTaskAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(func, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> InvokeTaskAsync<TArg, TResult>(Func<TArg, TResult> func, TArg arg, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(func, arg, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> InvokeTaskAsync<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> func, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(func, arg1, arg2, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> InvokeTaskAsync<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> func, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(func, arg1, arg2, arg3, cancellationToken));
}
