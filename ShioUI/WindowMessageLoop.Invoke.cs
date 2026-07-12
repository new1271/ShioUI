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
            return null;
        if (_threadIdLocal.Value == messageLoopThreadId)
            return @delegate.DynamicInvoke(null);
        return InvokeTaskCoreAsync(messageLoopThreadId, @delegate, null, CancellationToken.None).Result;
    }

    public static object? Invoke(Delegate @delegate, params object?[]? args)
    {
        uint messageLoopThreadId = InterlockedHelper.Read(ref _threadIdForMessageLoop);
        if (messageLoopThreadId == 0)
            InvalidOperationException.Throw();

        if (_threadIdLocal.Value == messageLoopThreadId)
            return @delegate.DynamicInvoke(args);
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
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        invokeMessageFilter.AddInvoke(new InvokeClosure(@delegate, args, null, cancellationToken));
        PostInvokeMessage(threadId);
    }

    private static Task<object?> InvokeTaskCoreAsync(uint threadId, Delegate @delegate, object?[]? args, CancellationToken cancellationToken = default)
    {
        InvokeMessageFilter? invokeMessageFilter = InterlockedHelper.Read(ref _invokeMessageFilter);
        if (invokeMessageFilter is null)
            InvalidOperationException.Throw();

        TaskCompletionSource<object?> completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        invokeMessageFilter.AddInvoke(new InvokeClosure(@delegate, args, completionSource, cancellationToken));
        PostInvokeMessage(threadId);
        return completionSource.Task;
    }

    private static void PostInvokeMessage(uint threadId)
    {
        if (MathHelper.ToBooleanUnsafe(InterlockedHelper.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt)))
            return;
        User32.PostThreadMessageW(threadId, CustomWindowMessages.ShioWindowInvoke, 0, 0);
        InterlockedHelper.Write(ref _invokeBarrier, Booleans.FalseInt);
    }
}
