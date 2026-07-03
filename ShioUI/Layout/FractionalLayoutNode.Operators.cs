using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using ShioUI.Layout.Internals.Fractional;

namespace ShioUI.Layout;

partial class FractionalLayoutNode
{
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FractionalLayoutNode(float value) => Fixed(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FractionalLayoutNode(LayoutNode value) => FromLayoutNode(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator LayoutNode(FractionalLayoutNode value) => value.ToLayoutNode();

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode operator +(FractionalLayoutNode variable) => variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode operator -(FractionalLayoutNode variable)
    {
        if (variable.IsEmpty)
            return variable;
        if (variable is FixedValueLayoutNode fixedVariable)
            return Fixed(-fixedVariable.Value);
        return new NegativeOperatorLayoutNode(variable);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode operator +(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (left.IsEmpty)
            return right;
        if (right.IsEmpty)
            return left;
        if (left is FixedValueLayoutNode fixedLeft && right is FixedValueLayoutNode fixedRight)
            return Fixed(fixedLeft.Value + fixedRight.Value);
        return new AddOperatorLayoutNode(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode operator -(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (ReferenceEquals(left, right))
            return Empty;
        if (left.IsEmpty)
            return -right;
        if (right.IsEmpty)
            return left;
        if (left is FixedValueLayoutNode fixedLeft && right is FixedValueLayoutNode fixedRight)
            return Fixed(fixedLeft.Value - fixedRight.Value);
        return new SubtractOperatorLayoutNode(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode operator *(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (left.IsEmpty || right.IsOne())
            return left;
        if (right.IsEmpty || left.IsOne())
            return right;
        if (left is FixedValueLayoutNode fixedLeft && right is FixedValueLayoutNode fixedRight)
            return Fixed(fixedLeft.Value * fixedRight.Value);
        return new MultiplyOperatorLayoutNode(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode operator /(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        if (right.IsEmpty)
            throw new DivideByZeroException();
        if (ReferenceEquals(left, right))
            return Fixed(1);
        if (left.IsEmpty || right.IsOne())
            return left;
        if (left is FixedValueLayoutNode fixedLeft && right is FixedValueLayoutNode fixedRight)
            return Fixed(fixedLeft.Value / fixedRight.Value);
        return new DivideOperatorLayoutNode(left, right);
    }

    private bool IsOne() => this is FixedValueLayoutNode fixedVariable && fixedVariable.Value == 1.0f;
}
