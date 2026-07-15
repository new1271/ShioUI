using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

public delegate float CustomFractionalComputeDelegate(in LayoutContext context);

partial class FractionalLayoutNode
{
    private static readonly ConcurrentDictionary<float, FractionalLayoutNode> _fixedValueNodeDict = new ConcurrentDictionary<float, FractionalLayoutNode>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Fixed(float value)
    {
        if (value == 0.0f || value == -0.0f)
            return Empty;
        return _fixedValueNodeDict.GetOrAdd(value, static key => new FixedValueLayoutNode.Fractional(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode FromLayoutNode(LayoutNode node)
    {
        if (node.IsEmpty)
            return Empty;
        if (node is Internals.FixedValueLayoutNode fixedValue)
            return Fixed(fixedValue.Value);
        return new ToFractionalLayoutNode(node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Custom(Func<float> computeFunc) => new SimpleCustomLayoutNode.Fractional(computeFunc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Custom(CustomFractionalComputeDelegate computeFunc) => new CustomLayoutNode.Fractional(computeFunc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Max(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (ReferenceEquals(left, right))
            return left;
        if (left is FixedValueLayoutNode.Fractional leftFixed && right is FixedValueLayoutNode.Fractional rightFixed)
            return leftFixed.Value > rightFixed.Value ? left : right;
        return new MaxLayoutNode.Fractional(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Min(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (ReferenceEquals(left, right))
            return left;
        if (left is FixedValueLayoutNode.Fractional leftFixed && right is FixedValueLayoutNode.Fractional rightFixed)
            return leftFixed.Value < rightFixed.Value ? left : right;
        return new MinLayoutNode.Fractional(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Floor(FractionalLayoutNode node) => new FloorLayoutNode.Fractional(node);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Ceiling(FractionalLayoutNode node) => new CeilingLayoutNode.Fractional(node);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Round(FractionalLayoutNode node) => new RoundLayoutNode.Fractional.Default(node);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Round(FractionalLayoutNode node, MidpointRounding midpointRounding) => new RoundLayoutNode.Fractional.Custom(node, midpointRounding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Truncate(FractionalLayoutNode node) => new TruncateLayoutNode.Fractional(node);
}
