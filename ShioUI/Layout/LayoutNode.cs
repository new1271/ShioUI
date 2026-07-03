using System.Runtime.CompilerServices;

using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

public abstract partial class LayoutNode : LayoutNodeBase
{
    public static readonly LayoutNode Empty = new FixedValueLayoutNode(0);

    private int _cachedResult;

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReferenceEquals(this, Empty);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compute(in LayoutContext context)
        => context.GetComputedValue(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int ComputeInternal(in LayoutContext context)
    {
        ulong timestamp = context.Timestamp;
        if (CheckCacheTimestamp(timestamp))
            return _cachedResult;
        int result = ComputeCore(context);
        UpdateCacheTimestamp(timestamp);
        _cachedResult = result;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal (int Result, bool Cached) ComputeInternalWithCached(in LayoutContext context)
    {
        ulong timestamp = context.Timestamp;
        if (CheckCacheTimestamp(timestamp))
            return (Result: _cachedResult, Cached: true);
        int result = ComputeCore(context);
        UpdateCacheTimestamp(timestamp);
        _cachedResult = result;
        return (Result: result, Cached: false);
    }

    protected abstract int ComputeCore(in LayoutContext context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Negative() => -this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Add(LayoutNode variable) => this + variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Subtract(LayoutNode variable) => this - variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Multiply(LayoutNode variable) => this * variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Divide(LayoutNode variable) => this / variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Max(LayoutNode variable) => Max(this, variable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode Min(LayoutNode variable) => Min(this, variable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FractionalLayoutNode ToFractionalLayoutNode() => FractionalLayoutNode.FromLayoutNode(this);
}
