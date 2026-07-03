using System;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class FloorLayoutNode : LayoutNode
{
    private readonly FractionalLayoutNode _node;

    public FloorLayoutNode(FractionalLayoutNode node) => _node = node;

    protected override int ComputeCore(in LayoutContext context)
        => MathI.Floor(context.GetComputedValue(_node));
}
