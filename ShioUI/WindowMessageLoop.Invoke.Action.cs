using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke(Action action)
        => ThrowWhenMessageLoopThreadNotExists(TryInvoke(action));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<TArg>(Action<TArg> action, TArg arg)
        => ThrowWhenMessageLoopThreadNotExists(TryInvoke(action, arg));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        => ThrowWhenMessageLoopThreadNotExists(TryInvoke(action, arg1, arg2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Invoke<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        => ThrowWhenMessageLoopThreadNotExists(TryInvoke(action, arg1, arg2, arg3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync(Action action, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(action, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync<TArg>(Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(action, arg, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(action, arg1, arg2, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeAsync(action, arg1, arg2, arg3, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task InvokeTaskAsync(Action action, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(action, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task InvokeTaskAsync<TArg>(Action<TArg> action, TArg arg, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(action, arg, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task InvokeTaskAsync<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(action, arg1, arg2, cancellationToken));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task InvokeTaskAsync<TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
        => ThrowWhenMessageLoopThreadNotExists(TryInvokeTaskAsync(action, arg1, arg2, arg3, cancellationToken));
}
