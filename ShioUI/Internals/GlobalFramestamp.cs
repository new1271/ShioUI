using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioUI.Internals;

internal static class GlobalFramestamp
{
    private static ulong _counter = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Generate()
    {
        ulong result = Atomics.Increment(ref _counter);
        DebugHelper.WriteLineIf($"{nameof(GlobalFramestamp)} reached the last unique value of framestamp!", result == 0);
        return result;
    }
}
