using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Internals;
using ShioUI.Internals.Native;

namespace ShioUI;

partial class WindowMessageLoop
{
    private static nuint _postInvokeResult = UnsafeHelper.GetMaxValue<nuint>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryPostInvokeClosure(uint threadId, IInvokeClosure closure)
    {
        InvokeMessageFilter.Instance.AddInvoke(closure);
        return TryPostInvokeMessage(threadId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryPostInvokeMessage(uint threadId)
    {
        bool result;

        if (MathHelper.ToBooleanUnsafe(Atomics.CompareExchange(ref _invokeBarrier, Booleans.TrueInt, Booleans.FalseInt)))
        {
            nuint lastResult = Atomics.Read(ref _postInvokeResult);
            if (lastResult <= Booleans.TrueNativeUnsigned)
                return MathHelper.ToBooleanUnsafe(lastResult);

            result = User32.PostThreadMessageW(threadId, CustomWindowMessages.ShioUI_WindowInvoke, 0, 0);
            Atomics.CompareExchange(ref _postInvokeResult, MathHelper.BooleanToNativeUnsigned(result), lastResult);
            return result;
        }
        Atomics.Write(ref _postInvokeResult, UnsafeHelper.GetMaxValue<nuint>());
        result = User32.PostThreadMessageW(threadId, CustomWindowMessages.ShioUI_WindowInvoke, 0, 0);
        Atomics.Write(ref _postInvokeResult, MathHelper.BooleanToNativeUnsigned(result));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ProcessAllInvoke() => InvokeMessageFilter.Instance.ProcessAllInvoke();
}
