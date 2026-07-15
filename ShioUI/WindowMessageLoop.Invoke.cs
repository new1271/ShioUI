using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Internals;
using ShioUI.Internals.Native;

namespace ShioUI;

partial class WindowMessageLoop
{
    public static object? Invoke(Delegate @delegate)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (CurrentThreadId == messageLoopThreadId)
        {
            ProcessAllInvoke(); 
            return @delegate.DynamicInvoke(null);
        }
        return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None).Result;
    }

    public static object? Invoke(Delegate @delegate, params object?[]? args)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (CurrentThreadId == messageLoopThreadId)
        {
            ProcessAllInvoke();
            return @delegate.DynamicInvoke(args);
        }
        return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, CancellationToken.None).Result;
    }

    public static void InvokeAsync(Delegate @delegate)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvokeAsync(Delegate @delegate, params object?[]? args)
        => InvokeAsync(@delegate, args, CancellationToken.None);

    public static void InvokeAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        InvokeCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
    }

    public static Task<object?> InvokeTaskAsync(Delegate @delegate)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<object?> InvokeTaskAsync(Delegate @delegate, params object?[]? args)
        => InvokeTaskAsync(@delegate, args, CancellationToken.None);

    public static Task<object?> InvokeTaskAsync(Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, args, cancellationToken);
    }

    private static void InvokeCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default) 
        => PostInvokeClosure(threadId, new InvokeClosure(@delegate, args, null, cancellationToken));

    private static Task<object?> InvokeTaskCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        PostInvokeClosure(threadId, new InvokeClosure(@delegate, args, completionSource, cancellationToken));
        return completionSource.Task;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PostInvokeClosure(uint threadId, IInvokeClosure closure)
    {
        InvokeMessageFilter.Instance.AddInvoke(closure);
        PostInvokeMessage(threadId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PostInvokeMessage(uint threadId)
    {
        if (MathHelper.ToBooleanUnsafe(InterlockedHelper.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt)))
            return;
        User32.PostThreadMessageW(threadId, CustomWindowMessages.ShioUI_WindowInvoke, 0, 0);
        InterlockedHelper.Write(ref _invokeBarrier, Booleans.FalseInt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ProcessAllInvoke() => InvokeMessageFilter.Instance.ProcessAllInvoke();
}
