using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Internals;
using ShioUI.Internals.Native;

namespace ShioUI;

partial class WindowMessageLoop
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PostInvokeClosure(uint threadId, IInvokeClosure closure)
    {
        InvokeMessageFilter.Instance.AddInvoke(closure);
        PostInvokeMessage(threadId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PostInvokeMessage(uint threadId)
    {
        if (MathHelper.ToBooleanUnsafe(Atomics.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt)))
            return;
        User32.PostThreadMessageW(threadId, CustomWindowMessages.ShioUI_WindowInvoke, 0, 0);
        Atomics.Write(ref _invokeBarrier, Booleans.FalseInt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ProcessAllInvoke() => InvokeMessageFilter.Instance.ProcessAllInvoke();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowWhenMessageLoopThreadNotExists(bool condition)
    {
        if (condition)
            ThrowWhenMessageLoopThreadNotExists();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ThrowWhenMessageLoopThreadNotExists<T>(T? condition) where T : class
        => condition ?? ThrowWhenMessageLoopThreadNotExists<T>();

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static T ThrowWhenMessageLoopThreadNotExists<T>()
        => throw new InvalidOperationException("The message loop thread is not exists");

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowWhenMessageLoopThreadNotExists()
        => throw new InvalidOperationException("The message loop thread is not exists");
}
