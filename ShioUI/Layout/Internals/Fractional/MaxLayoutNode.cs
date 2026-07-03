using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class MaxLayoutNode : FractionalLayoutNode
{
    private readonly FractionalLayoutNode _leftVariable, _rightVariable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MaxLayoutNode(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        _leftVariable = left;
        _rightVariable = right;
    }

    protected override float ComputeCore(in LayoutContext context)
        => MathHelper.Max(context.GetComputedValue(_leftVariable), context.GetComputedValue(_rightVariable));
}
