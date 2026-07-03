using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class AddOperatorLayoutNode : FractionalLayoutNode
{
    private readonly FractionalLayoutNode _leftVariable, _rightVariable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AddOperatorLayoutNode(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        _leftVariable = left;
        _rightVariable = right;
    }

    protected override float ComputeCore(in LayoutContext context)
        => context.GetComputedValue(_leftVariable) + context.GetComputedValue(_rightVariable);
}
