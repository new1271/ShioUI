using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using ShioUI.Layout.Internals.Fractional;

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
        return _fixedValueNodeDict.GetOrAdd(value, static key => new FixedValueLayoutNode(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode FromLayoutNode(LayoutNode node)
    {
        if (node.IsEmpty)
            return Empty;
        if (node is Internals.FixedValueLayoutNode fixedValue)
            return Fixed(fixedValue.Value);
        return new ConvertLayoutNode(node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Custom(Func<float> computeFunc) => new SimpleCustomLayoutNode(computeFunc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Custom(CustomComputeDelegate computeFunc) => new CustomLayoutNode(computeFunc);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Max(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (ReferenceEquals(left, right))
            return left;
        if (left is FixedValueLayoutNode leftFixed && right is FixedValueLayoutNode rightFixed)
            return leftFixed.Value > rightFixed.Value ? left : right;
        return new MaxLayoutNode(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode Min(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (ReferenceEquals(left, right))
            return left;
        if (left is FixedValueLayoutNode leftFixed && right is FixedValueLayoutNode rightFixed)
            return leftFixed.Value < rightFixed.Value ? left : right;
        return new MinLayoutNode(left, right);
    }
}
