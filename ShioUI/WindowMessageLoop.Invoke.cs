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
}
