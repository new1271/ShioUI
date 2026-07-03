using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class MinLayoutNode : FractionalLayoutNode
{
    private readonly FractionalLayoutNode _leftVariable, _rightVariable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MinLayoutNode(FractionalLayoutNode left, FractionalLayoutNode right)
    {
        _leftVariable = left;
        _rightVariable = right;
    }

    protected override float ComputeCore(in LayoutContext context)
        => MathHelper.Min(context.GetComputedValue(_leftVariable), context.GetComputedValue(_rightVariable));
}
