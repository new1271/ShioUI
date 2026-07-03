using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace ShioUI.Layout;

public abstract class LayoutNodeBase
{
    private static int _identifierCounter = 0;

    private readonly int _identifier;
    private ulong _timestamp;

    public int NodeId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _identifier;
    }

    protected LayoutNodeBase() => _identifier = InterlockedHelper.Increment(ref _identifierCounter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool CheckCacheTimestamp(ulong timestamp) => timestamp != 0 && timestamp == _timestamp;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void UpdateCacheTimestamp(ulong timestamp) => _timestamp = timestamp;

    public override int GetHashCode() => _identifier;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearCache() => _timestamp = 0;
}
