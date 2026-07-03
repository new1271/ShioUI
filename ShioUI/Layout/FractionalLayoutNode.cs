using System;
using System.Runtime.CompilerServices;

using ShioUI.Layout.Internals.Fractional;

namespace ShioUI.Layout;

public abstract partial class FractionalLayoutNode : LayoutNodeBase
{
    public static readonly FractionalLayoutNode Empty = new FixedValueLayoutNode(0);

    private float _cachedResult;

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReferenceEquals(this, Empty);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Compute(in LayoutContext context)
        => context.GetComputedValue(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float ComputeInternal(in LayoutContext context)
    {
        ulong timestamp = context.Timestamp;
        if (CheckCacheTimestamp(timestamp))
            return _cachedResult;
        float result = ComputeCore(context);
        UpdateCacheTimestamp(timestamp);
        _cachedResult = result;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal (float Result, bool Cached) ComputeInternalWithCached(in LayoutContext context)
    {
        ulong timestamp = context.Timestamp;
        if (CheckCacheTimestamp(timestamp))
            return (Result: _cachedResult, Cached: true);
        float result = ComputeCore(context);
        UpdateCacheTimestamp(timestamp);
        _cachedResult = result;
        return (Result: result, Cached: false);
    }

    protected abstract float ComputeCore(in LayoutContext context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FractionalLayoutNode Max(FractionalLayoutNode variable) => Max(this, variable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FractionalLayoutNode Min(FractionalLayoutNode variable) => Min(this, variable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Floor() => new FloorLayoutNode(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Ceiling() => new CeilingLayoutNode(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Round() => new RoundLayoutNode.Default(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Round(MidpointRounding midpointRounding) => new RoundLayoutNode.Custom(this, midpointRounding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Truncate() => new TruncateLayoutNode(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode ToLayoutNode() => new TruncateLayoutNode(this);
}
